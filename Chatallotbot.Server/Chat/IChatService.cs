namespace Chatallotbot.Server.Chat;

public interface IChatService
{
    Task<List<ChatMessageDto>> SendMessage (List<ChatMessageDto> request, CancellationToken cancellationToken);
}