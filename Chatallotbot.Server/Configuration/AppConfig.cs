namespace Chatallotbot.Server.Configuration;

public static class AppConfig
{
    public static OpenAiConfiguration EmbeddingConfig { get; private set; } = null!;
    public static OpenAiConfiguration ChatConfig { get; private set; } = null!;
    public static ChatSettings ChatSettings { get; private set; } = new();

    public static void Initialize(ConfigurationManager configuration)
    {
        EmbeddingConfig = configuration.GetSection("OpenAIServiceOptions:Embedding").Get<OpenAiConfiguration>() ??
                          throw new InvalidOperationException("Embedding configuration not found");
        ChatConfig = configuration.GetSection("OpenAIServiceOptions:Chat").Get<OpenAiConfiguration>() ??
                          throw new InvalidOperationException("Chat configuration not found");
        ChatSettings = configuration.GetSection("ChatSettings").Get<ChatSettings>() ??
                       throw new InvalidOperationException("ChatSettings not found");
        
        if(!EmbeddingConfig.IsValid())
            throw new InvalidOperationException("Embeddings configuration is not properly set.");
        
        if(!ChatConfig.IsValid())
            throw new InvalidOperationException("Chat configuration is not properly set.");

    }
}

public record OpenAiConfiguration
{
    public string Endpoint { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public bool IsValid() => !string.IsNullOrWhiteSpace(Endpoint) &&
                             !string.IsNullOrWhiteSpace(Model) &&
                             !string.IsNullOrWhiteSpace(Key);
};

public record ChatSettings
{
    public int MaxRequestLength { get; init; } = 1000;
}