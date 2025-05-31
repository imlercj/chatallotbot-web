namespace Chatallotbot.Server.Configuration;

public static class AppConfig
{
    public static OpenAiConfiguration OpenAiConfig { get; private set; } = null!;
    public static ChatSettings ChatSettings { get; private set; } = new();

    public static void Initialize(ConfigurationManager configuration)
    {
        OpenAiConfig = configuration.GetSection("AzureResources:OpenAIServiceOptions").Get<OpenAiConfiguration>() ??
                       throw new InvalidOperationException("OpenAiConfiguration not found");
        ChatSettings = configuration.GetSection("ChatSettings").Get<ChatSettings>() ??
                       throw new InvalidOperationException("ChatSettings not found");

        if (string.IsNullOrWhiteSpace(OpenAiConfig.Endpoint) ||
            string.IsNullOrWhiteSpace(OpenAiConfig.DeploymentOrModelName) /*||
            string.IsNullOrWhiteSpace(OpenAiConfig.Key)*/)
            throw new InvalidOperationException("OpenAI configuration is not properly set.");
    }
}

public record OpenAiConfiguration
{
    public string Endpoint { get; init; } = string.Empty;
    public string DeploymentOrModelName { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
};

public record ChatSettings
{
    public int MaxRequestLength { get; init; } = 1000;
}