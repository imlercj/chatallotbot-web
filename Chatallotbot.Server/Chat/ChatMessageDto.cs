using System.Text.Json.Serialization;

namespace Chatallotbot.Server.Chat;

public record ChatMessageDto
{
    public ChatMessageDto() : this(ChatRoleDto.User, string.Empty) { }

    public ChatMessageDto(ChatRoleDto role, string content)
    {
        Role = role;
        Content = content;
    }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChatRoleDto Role { get; init; }
    public string Content { get; init; } = string.Empty;
}