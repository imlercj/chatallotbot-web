namespace Chatallotbot.Server.Chat;

public interface IChatService
{
    ChatResponse ResetChatHistory();
    Task<ChatResponse> SendMessage (ChatRequest request, CancellationToken cancellationToken);
}