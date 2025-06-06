namespace Chatallotbot.Server.Chat;

public record ChatMessageDto
{
    public string Role { get; set; }
    public string Content { get; set; }
}