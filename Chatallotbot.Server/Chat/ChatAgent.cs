using System.Diagnostics.CodeAnalysis;
using Azure.AI.OpenAI;
using Azure.Identity;
using Chatallotbot.Server.Configuration;
using Chatallotbot.Server.Exceptions;
using Chatallotbot.Server.Services;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Microsoft.SemanticKernel.Data;

namespace Chatallotbot.Server.Chat;

public class ChatAgent(MsiAuth msiAuth)
{
    // Create a vector store to store documents.
    private readonly PostgresVectorStore _vectorStore = new(AppConfig.ConnectionStrings.PostgresDb,
        new PostgresVectorStoreOptions
        {
            EmbeddingGenerator = new AzureOpenAIClient(
                    new Uri(AppConfig.EmbeddingConfig.Endpoint),
                    msiAuth.AzureCredentials)
                .GetEmbeddingClient(AppConfig.EmbeddingConfig.Model)
                .AsIEmbeddingGenerator(AppConfig.EmbeddingConfig.Dimensions)
        });
    
    // Create an agent.
    private readonly ChatCompletionAgent _agent = new()
    {
        Name = "FriendlyAssistant",
        Instructions = "You are a friendly assistant",
        Kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: AppConfig.ChatConfig.Model,
                endpoint: AppConfig.ChatConfig.Endpoint,
                credentials: msiAuth.AzureCredentials)
            .Build()
    };

    [Experimental("SKEXP0130")]
    public async Task<ChatHistory> Chat(ChatHistory history, CancellationToken cancellationToken)
    {
        if(history.Last().Content is null)
            throw new ArgumentNullException(nameof(history), "Message cannot be empty.");
        
        var tableName = "public.fitjar";
        // Create a TextSearchStore for storing and searching text documents.
        using var textSearchStore = new TextSearchStore<string>(_vectorStore, collectionName: tableName,
            vectorDimensions: AppConfig.EmbeddingConfig.Dimensions);
        
        var option = new TextSearchOptions
        {
            IncludeTotalCount = true
        };
        var testResults = await textSearchStore.SearchAsync("Butikk", option, cancellationToken: cancellationToken);
        
        // Create an agent thread and add the TextSearchProvider.
        ChatHistoryAgentThread agentThread = new(history);
        var textSearchProvider = new TextSearchProvider(textSearchStore);
        agentThread.AIContextProviders.Add(textSearchProvider);

        // Use the agent with RAG capabilities.
        ChatMessageContent response = await _agent
            .InvokeAsync(history.Last().Content!, agentThread, cancellationToken: cancellationToken)
            .FirstAsync(cancellationToken: cancellationToken);
        Console.WriteLine(response.Content);
        
        if (string.IsNullOrWhiteSpace(response.Content))
            throw new EmptyChatResponse();
        
        history.AddAssistantMessage(response.Content);
        
        return history;
    }
}