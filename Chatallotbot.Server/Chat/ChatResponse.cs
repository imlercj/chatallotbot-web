namespace Chatallotbot.Server.Chat;

public class ChatResponse
{
    public required List<ChatMessageDto> Chat { get; set; }
    public long TotalTokens { get; set; }
}