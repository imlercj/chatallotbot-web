using Azure.AI.OpenAI;
using Chatallotbot.Server.Configuration;
using Chatallotbot.Server.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.PgVector;

namespace Chatallotbot.Server.Chat;

public interface IVectorStoreFactory
{
    VectorStore CreateVectorStore();
}

public class InMemoryVectorStoreFactory(MsiAuth msiAuth) : IVectorStoreFactory
{
    public VectorStore CreateVectorStore()
        => new InMemoryVectorStore(
            new InMemoryVectorStoreOptions
            {
                EmbeddingGenerator = new AzureOpenAIClient(
                        new Uri(AppConfig.EmbeddingConfig.Endpoint),
                        msiAuth.AzureCredentials)
                    .GetEmbeddingClient(AppConfig.EmbeddingConfig.Model)
                    .AsIEmbeddingGenerator(AppConfig.EmbeddingConfig.Dimensions)
            });
}

public class PostgresVectorStoreFactory(MsiAuth msiAuth) : IVectorStoreFactory
{
    public VectorStore CreateVectorStore()
        => new PostgresVectorStore(
            AppConfig.ConnectionStrings.PostgresDb,
            new PostgresVectorStoreOptions
            {
                EmbeddingGenerator = new AzureOpenAIClient(
                        new Uri(AppConfig.EmbeddingConfig.Endpoint),
                        msiAuth.AzureCredentials)
                    .GetEmbeddingClient(AppConfig.EmbeddingConfig.Model)
                    .AsIEmbeddingGenerator(AppConfig.EmbeddingConfig.Dimensions)
            });
}