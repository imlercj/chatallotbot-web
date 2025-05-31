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
    
    private readonly ChatMessage _initialChatMessage = new(ChatRole.System,
        """
            You are a friendly hiking enthusiast who helps people discover fun hikes in their area.
            You introduce yourself when first saying hello.
            When helping people out, you always ask them for this information
            to inform the hiking recommendation you provide:

            1. The location where they would like to hike
            2. What hiking intensity they are looking for

            You will then provide three suggestions for nearby hikes that vary in length
            after you get that information. You will also share an interesting fact about
            the local nature on the hikes when making a recommendation. At the end of your
            response, ask if there is anything else you can help with.
        """);

    private readonly List<ChatMessage> _chatHistory = [];

    public ChatResponse ResetChatHistory()
    {
        _chatHistory.Clear();
        
        _chatHistory.Add(_initialChatMessage);
        return new ChatResponse { Reply = _initialChatMessage.Text };
    }

    public async Task<ChatResponse> SendMessage(ChatRequest request, CancellationToken cancellationToken)
    {
        _chatHistory.Add(new ChatMessage(ChatRole.User, request.Message));
        var response = "";
        await foreach (var item in _chatClient.GetStreamingResponseAsync(_chatHistory,
                           cancellationToken: cancellationToken))
            response += item.Text;
        _chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));

        if (string.IsNullOrWhiteSpace(response))
            throw new EmptyChatResponse();

        return new ChatResponse { Reply = response };
    }
}