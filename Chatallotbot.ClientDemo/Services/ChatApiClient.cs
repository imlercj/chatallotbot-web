using System.Text.Json;
using System.Text.Json.Serialization;
using Chatallotbot.Server.Chat;

namespace Chatallotbot.ClientDemo.Services;

public class ChatApiClient(HttpClient httpClient)
{
    private const string ServicePath = "chat";

    private JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<ChatResponse> SendMessages(List<ChatMessageDto> chatMessages)
    {
        var apiKey = "mTx-Eycp_XpUZIU0gvWKKIAFkJsoegkDEZ-UuAxj8_A";
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("X-Chatallot-Key", apiKey);
        
        var serializedContent = JsonSerializer.Serialize(chatMessages);
        var stringContent = new StringContent(serializedContent, System.Text.Encoding.UTF8, "application/json");

        var httpResponse = await httpClient.PostAsync(ServicePath, stringContent);
        if (!httpResponse.IsSuccessStatusCode)
            throw new Exception($"{httpResponse.StatusCode} - {httpResponse.ReasonPhrase}");

        var stringResponse = await httpResponse.Content.ReadAsStreamAsync();
        var response = await JsonSerializer.DeserializeAsync<ChatResponse>(stringResponse, JsonOptions);
        if (response is null) throw new InvalidOperationException("Response from chat service is null.");
        return response;
    }
}