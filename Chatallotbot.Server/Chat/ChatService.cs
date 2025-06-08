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
    

    public async Task<ChatResponse> SendMessage(List<ChatMessageDto> request, CancellationToken cancellationToken)
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
            .Select(dto => new ChatMessage(new ChatRole(dto.Role.ToString().ToLower()),
                dto.Content))
            .ToList();
        
        
        var response = await _chatClient.GetResponseAsync(chatHistory, cancellationToken: cancellationToken);
        var responseContent = response.Text;
        if (string.IsNullOrWhiteSpace(responseContent))
            throw new EmptyChatResponse();

        request.Add(new ChatMessageDto
        {
            Role = ChatRoleDto.Assistant,
            Content = responseContent
        });

        return new ChatResponse
        {
            ChatMessages = request,
            TotalTokens = response.Usage?.TotalTokenCount ?? 0
        };
    }
}