using System.ClientModel;
using Azure.AI.OpenAI;
using Chatallotbot.Server.Configuration;
using Chatallotbot.Server.Services;
using OpenAI.Embeddings;

namespace Chatallotbot.Server.Chat;

public class ChatallotEmbeddingClient(MsiAuth msiAuth)
{
    private readonly EmbeddingClient _embeddingClient = new AzureOpenAIClient(
            new Uri(AppConfig.EmbeddingConfig.Endpoint),
            msiAuth.AzureCredentials)
        .GetEmbeddingClient(AppConfig.EmbeddingConfig.Model);

    private readonly EmbeddingGenerationOptions _options = new()
    {
        Dimensions = 768
    };

    public Task<ClientResult<OpenAIEmbedding>>
        GenerateEmbeddingAsync(string input, CancellationToken cancellationToken) =>
        _embeddingClient.GenerateEmbeddingAsync(input, _options, cancellationToken);
}