using Azure.AI.OpenAI;
using Chatallotbot.Server.Configuration;
using Chatallotbot.Server.Services;
using Microsoft.Extensions.AI;

namespace Chatallotbot.Server.Chat;

public class ChatallotChatClient(MsiAuth msiAuth)
{
    private readonly IChatClient _chatClient = new AzureOpenAIClient(new Uri(AppConfig.ChatConfig.Endpoint),
            msiAuth.AzureCredentials)
        .GetChatClient(AppConfig.ChatConfig.Model)
        .AsIChatClient();

    public Task<Microsoft.Extensions.AI.ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken) =>
        _chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
}