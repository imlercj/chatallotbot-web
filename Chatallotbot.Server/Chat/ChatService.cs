using System.Globalization;
using Chatallotbot.Server.Data;
using Chatallotbot.Server.Exceptions;
using Microsoft.Extensions.AI;
using System.Linq;
namespace Chatallotbot.Server.Chat;

public class ChatService(
    ChatallotEmbeddingClient embeddingClient,
    ChatallotChatClient chatClient) : IChatService
{
    public async Task<ChatResponse> SendMessage(List<ChatMessageDto> request, CancellationToken cancellationToken)
    {
        // Create embedding context
        var clientResult = await embeddingClient.GenerateEmbeddingAsync(request.Last().Content, cancellationToken);
        var vector = clientResult.Value.ToFloats();
        var vectorString = "[";
        foreach (var item in vector.Span)
            vectorString += item.ToString(CultureInfo.InvariantCulture) + ", ";
        vectorString = vectorString.TrimEnd(',', ' ') + "]";

        // Get relevant documents
        var tableName = "public.lindesneskommunelarge";
        var retrievedDocs = await PostgresService.GetEmbeddingsDataAsync(tableName, vectorString, 10);
        Console.WriteLine(string.Join(", ", retrievedDocs.Select(doc => doc.Metadata.Url)));

        // Get the last message from the request
        var lastRequest = request.Last();
        if (lastRequest.Role is ChatRoleDto.User)
            request[^1] = new ChatMessageDto
            {
                Role = lastRequest.Role,
                Content = "\n\nRelevant documents:\n" +
                          string.Join("\n", retrievedDocs.Select(doc => $"{doc.Metadata.Url} \n {doc.Content}")) +
                          lastRequest.Content
            };
        
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