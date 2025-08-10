using System.Diagnostics.CodeAnalysis;
using Chatallotbot.Server.Configuration;
using Chatallotbot.Server.Exceptions;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Data;

namespace Chatallotbot.Server.Chat;

public class ChatAgent(
    IVectorStoreFactory vectorStoreFactory,
    IChatCompletionAgentFactory chatCompletionAgentFactory)
{
    // Create a vector store to store documents.
    private readonly VectorStore _vectorStore = vectorStoreFactory.CreateVectorStore();
    private readonly ChatCompletionAgent _agent = chatCompletionAgentFactory.CreateAgent();

    [Experimental("SKEXP0130")]
    public async Task<ChatHistory> Chat(string message, ChatHistory history, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(message))
            throw new ArgumentNullException(nameof(message), "Message cannot be empty.");
        
        var tableName = "public.lindesneskommunelarge";
        // Create a TextSearchStore for storing and searching text documents.
        using var textSearchStore = new TextSearchStore<string>(_vectorStore, collectionName: tableName,
            vectorDimensions: AppConfig.EmbeddingConfig.Dimensions);

        // Check if there are documents in the store
        var searchOptions = new TextSearchOptions { IncludeTotalCount = true };
        var searchResult = await textSearchStore.SearchAsync(message, searchOptions, cancellationToken: cancellationToken);
        
        var hasResults = await searchResult.Results.AnyAsync(cancellationToken);
        if (!hasResults)
        {
            history.AddAssistantMessage("I don't have information about this in my knowledge base.");
            return history;
        }

        // Create an agent thread and add the TextSearchProvider.
        ChatHistoryAgentThread agentThread = new();
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