using System.Text.Json;
using Azure;
using Chatallotbot.Server.Configuration;
using Chatallotbot.Server.Exceptions;
using Azure.AI.OpenAI;
using Chatallotbot.Server.Services;
using Microsoft.Extensions.AI;
using OpenAI.Embeddings;


namespace Chatallotbot.Server.Chat;

public class ChatService(MsiAuth msiAuth) : IChatService
{
    private readonly EmbeddingClient _embeddingClient = new AzureOpenAIClient(new Uri(AppConfig.EmbeddingConfig.Endpoint),
            msiAuth.AzureCredentials)
        .GetEmbeddingClient(AppConfig.EmbeddingConfig.Model);
 
    private readonly IChatClient _chatClient = new AzureOpenAIClient(new Uri(AppConfig.ChatConfig.Endpoint),
            msiAuth.AzureCredentials)
        .GetChatClient(AppConfig.ChatConfig.Model)
        .AsIChatClient();
    

    public async Task<List<ChatMessageDto>> SendMessage(List<ChatMessageDto> request, CancellationToken cancellationToken)
    {
        // Embedding
        var options = new OpenAI.Embeddings.EmbeddingGenerationOptions
        {
            Dimensions = 768
        };

        var clientResult = await _embeddingClient.GenerateEmbeddingAsync(request.Last().Content, options, cancellationToken);
        var clientResponse = clientResult.Value;
        
        // Talk to Postgress
        
        // Get the last message from the request
        var chatHistory = request
            .Select(dto => new ChatMessage(new ChatRole(dto.Role.ToLower()),
                dto.Content))
            .ToList();
        
        var response = "";
        await foreach (var item in _chatClient.GetStreamingResponseAsync(chatHistory,
                           cancellationToken: cancellationToken))
            response += item.Text;

        if (string.IsNullOrWhiteSpace(response))
            throw new EmptyChatResponse();

        request.Add(new ChatMessageDto
        {
            Role = ChatRole.Assistant.ToString(),
            Content = response
        });

        return request;
    }
}