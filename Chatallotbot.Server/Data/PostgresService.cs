using System.Text.Json;
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
    private static JsonSerializerOptions JsonSerializerOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task<List<EmbeddingSearchResult>> GetEmbeddingsDataAsync(string tableName, string queryEmbedding,
        int take = 3)
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

        var results = new List<EmbeddingSearchResult>();

        while (await reader.ReadAsync())
            results.Add(new EmbeddingSearchResult
            {
                Content = reader.GetString(0),
                Metadata = JsonSerializer.Deserialize<DocumentMetadata>(reader.GetString(1), JsonSerializerOptions) ??
                           new DocumentMetadata(),
                Similarity = reader.GetFloat(2)
            });

        return results;
    }
}

public record EmbeddingSearchResult
{
    public string Content { get; init; } = string.Empty;
    public DocumentMetadata Metadata { get; init; } = new();
    public float Similarity { get; init; }
}

public record DocumentMetadata
{
    public string? Header1 { get; init; }
    public string? Header2 { get; init; }
    public string? Url { get; init; }
}