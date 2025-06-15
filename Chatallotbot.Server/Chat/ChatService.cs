using Chatallotbot.Server.Data;
using Chatallotbot.Server.Exceptions;
using Microsoft.Extensions.AI;

namespace Chatallotbot.Server.Chat;

public class ChatService(
    ChatallotEmbeddingClient embeddingClient,
    ChatallotChatClient chatClient,
    PostgresService postgresService) : IChatService
{
    public async Task<ChatResponse> SendMessage(List<ChatMessageDto> request, CancellationToken cancellationToken)
    {
        var clientResult = await embeddingClient.GenerateEmbeddingAsync(request.Last().Content, cancellationToken);
        var embedding = clientResult.Value;

        var vector = embedding.ToFloats();
        var length = vector.Length;
        Console.Write($"data[{embedding.Index}]: length={length}, ");
        Console.Write($"[{vector.Span[0]}, {vector.Span[1]}, ..., ");
        Console.WriteLine($"{vector.Span[length - 2]}, {vector.Span[length - 1]}]");

        // Talk to Postgress
        var tableName = "public.lindesneskommunelarge";
        var retrievedDocs = await postgresService.GetEmbeddingsDataAsync(tableName, embedding.ToString() ?? "", 10);
        Console.WriteLine(string.Join(", ", retrievedDocs.Select(doc => doc["metadata"])));

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