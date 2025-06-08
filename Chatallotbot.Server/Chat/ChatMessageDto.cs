namespace Chatallotbot.Server.Chat;

public record ChatMessageDto(string Role, string Content)
{
    public ChatMessageDto() : this("User", string.Empty)
    {
    }
}