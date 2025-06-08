namespace Chatallotbot.Server.Chat;

public class ChatResponse
{
    public List<ChatMessageDto> ChatMessages { get; set; }
    public long TotalTokens { get; set; }
}