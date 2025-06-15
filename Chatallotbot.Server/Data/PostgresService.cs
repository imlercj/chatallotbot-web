using Chatallotbot.Server.Configuration;
using Npgsql;

namespace Chatallotbot.Server.Data;

public class PostgresService
{
    /*public async Task<string> GetCustomerTableNameAsync(string tableName)
    {
        await using var conn = new NpgsqlConnection(AppConfig.ConnectionStrings.PostgresDb);
        await conn.OpenAsync();
        
        var query = "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = @tableName)";
        await using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("tableName", tableName);
        
        var exists = (bool)await cmd.ExecuteScalarAsync();
        return exists ? tableName : string.Empty;
    }*/
    public async Task<List<Dictionary<string, object>>> GetEmbeddingsDataAsync(string tableName, string queryEmbedding, int take = 3)
    {
        var query = $"""
                     SELECT
                         document,
                         metadata,
                         (1 - (embedding <=> '{queryEmbedding}'))
                             AS similarity
                     FROM {tableName}
                     ORDER BY similarity DESC
                     LIMIT {take}
                     """;
        
        await using var conn = new NpgsqlConnection(AppConfig.ConnectionStrings.PostgresDb);
        await conn.OpenAsync();
        
        await using var cmd = new NpgsqlCommand(query, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        
        var results = new List<Dictionary<string, object>>();

        while (await reader.ReadAsync())
            results.Add(new Dictionary<string, object>
            {
                { "content", reader.GetString(0) },
                { "metadata", reader.GetString(1) },
                { "similarity", reader.GetFloat(2) }
            });

        return results;
    }
}