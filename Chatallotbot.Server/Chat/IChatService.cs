namespace Chatallotbot.Server.Chat;

public interface IChatService
{
    Task<ChatResponse> SendMessage (List<ChatMessageDto> request, CancellationToken cancellationToken);
}