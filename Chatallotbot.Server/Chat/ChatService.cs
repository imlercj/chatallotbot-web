using System.Text.Json;
using Azure;
using Chatallotbot.Server.Configuration;
using Chatallotbot.Server.Exceptions;
using Azure.AI.OpenAI;
using Chatallotbot.Server.Services;
using Microsoft.Extensions.AI;

namespace Chatallotbot.Server.Chat;

public class ChatService(/*MsiAuth msiAuth*/) : IChatService
{
    private readonly IChatClient _chatClient =
        new AzureOpenAIClient(new Uri(AppConfig.OpenAiConfig.Endpoint), new AzureKeyCredential(AppConfig.OpenAiConfig.Key)/*msiAuth.AzureCredentials*/)// new AzureKeyCredential(openAiConfig.Key));
            .GetChatClient(AppConfig.OpenAiConfig.DeploymentOrModelName)
            .AsIChatClient();

    public async Task<List<ChatMessageDto>> SendMessage(List<ChatMessageDto> request, CancellationToken cancellationToken)
    {
        var chatHistory = request
            .Select(dto => new ChatMessage(new ChatRole(dto.Role.ToLower()),
                dto.Content))
            .ToList();
        
        var response = "";
        await foreach (var item in _chatClient.GetStreamingResponseAsync(chatHistory,
                           cancellationToken: cancellationToken))
            response += item.Text;

        if (string.IsNullOrWhiteSpace(response))
            throw new EmptyChatResponse();

        request.Add(new ChatMessageDto
        {
            Role = ChatRole.Assistant.ToString(),
            Content = response
        });

        return request;
    }
}