using Chatallotbot.Server.Exceptions;
using Microsoft.Extensions.AI;

namespace Chatallotbot.Server.Chat;

public class ChatService(ChatallotEmbeddingClient embeddingClient, ChatallotChatClient chatClient) : IChatService
{
    public async Task<ChatResponse> SendMessage(List<ChatMessageDto> request, CancellationToken cancellationToken)
    {
        var clientResult = await embeddingClient.GenerateEmbeddingAsync(request.Last().Content, cancellationToken);
        var clientResponse = clientResult.Value;
        
        // Talk to Postgress
        
        // Get the last message from the request
        var chatHistory = request
            .Select(dto => new ChatMessage(new ChatRole(dto.Role.ToString().ToLower()),
                dto.Content))
            .ToList();
        
        
        var response = await chatClient.GetResponseAsync(chatHistory, cancellationToken: cancellationToken);
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