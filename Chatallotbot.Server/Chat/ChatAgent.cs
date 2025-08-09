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
        Name = "KnowledgeBaseAssistant",
        Instructions = """
                       You are an assistant that ONLY provides information found in the retrieved documents.
                       If the information cannot be found in the retrieved documents, say 'I don't have information about this in my knowledge base.'
                       Do not make up or infer information that isn't explicitly stated in the retrieved documents.
                       Always cite the source of your information when possible.
                       """,
        Kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: AppConfig.ChatConfig.Model,
                endpoint: AppConfig.ChatConfig.Endpoint,
                credentials: msiAuth.AzureCredentials)
            .Build()
    };

    [Experimental("SKEXP0130")]
    public async Task<ChatHistory> Chat(string message, ChatHistory history, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(message))
            throw new ArgumentNullException(nameof(message), "Message cannot be empty.");
        
        var tableName = "public.fitjar";
        // Create a TextSearchStore for storing and searching text documents.
        using var textSearchStore = new TextSearchStore<string>(_vectorStore, collectionName: tableName,
            vectorDimensions: AppConfig.EmbeddingConfig.Dimensions);

        // Check if there are documents in the store
        var searchOptions = new TextSearchOptions { IncludeTotalCount = true };
        var relevantDocs = await textSearchStore.SearchAsync(message, searchOptions, cancellationToken: cancellationToken);
        
        var results = relevantDocs.Results;
        var test = await results.AnyAsync(cancellationToken: cancellationToken);
        /*if (!relevantDocs.Any())
        {
            history.AddAssistantMessage("I don't have information about this in my knowledge base.");
            return history;
        }*/

        // Create an agent thread and add the TextSearchProvider.
        ChatHistoryAgentThread agentThread = new(history);
        var textSearchProvider = new TextSearchProvider(textSearchStore);
        agentThread.AIContextProviders.Add(textSearchProvider);

        // Use the agent with RAG capabilities.
        ChatMessageContent response = await _agent
            .InvokeAsync(message, agentThread, cancellationToken: cancellationToken)
            .FirstAsync(cancellationToken: cancellationToken);
        
        if (string.IsNullOrWhiteSpace(response.Content))
            throw new EmptyChatResponse();
        
        history.AddAssistantMessage(response.Content);
        
        return history;
    }
}