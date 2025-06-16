namespace Chatallotbot.Server.Configuration;

public static class AppConfig
{
    public static ConnectionStrings ConnectionStrings { get; private set; } = null!;
    public static EmbeddingConfiguration EmbeddingConfig { get; private set; } = null!;
    public static OpenAiConfiguration ChatConfig { get; private set; } = null!;
    public static ChatSettings ChatSettings { get; private set; } = new();
    public static ApiSecurity ApiSecurity { get; private set; } = new();

    public static void Initialize(ConfigurationManager configuration)
    {
        ConnectionStrings = configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>() ??
                            throw new InvalidOperationException("Connection strings not found");
        var tempEmbeddingConfig =
            configuration.GetSection("OpenAIServiceOptions:Embedding").Get<EmbeddingConfiguration>() ??
            throw new InvalidOperationException("Embedding configuration not found");
        EmbeddingConfig = configuration.GetSection("Embedding").Get<EmbeddingConfiguration>() ??
                          throw new InvalidOperationException("Embedding configuration not found");
        EmbeddingConfig.Endpoint = tempEmbeddingConfig.Endpoint;
        EmbeddingConfig.Model = tempEmbeddingConfig.Model;
        EmbeddingConfig.Key = tempEmbeddingConfig.Key;

        ChatConfig = configuration.GetSection("OpenAIServiceOptions:Chat").Get<OpenAiConfiguration>() ??
                     throw new InvalidOperationException("Chat configuration not found");
        ChatSettings = configuration.GetSection("ChatSettings").Get<ChatSettings>() ??
                       throw new InvalidOperationException("ChatSettings not found");
        ApiSecurity = configuration.GetSection("ApiSecurity").Get<ApiSecurity>() ??
                      throw new InvalidOperationException("ApiSecurity not found");

        ValidateConfigurations();
    }

    private static void ValidateConfigurations()
    {
        if (!ConnectionStrings.IsValid())
            throw new InvalidOperationException("Connection strings are not properly set.");

        if (!EmbeddingConfig.IsValid())
            throw new InvalidOperationException("Embedding configuration is not properly set.");

        if (!ChatConfig.IsValid())
            throw new InvalidOperationException("Chat configuration is not properly set.");
    }
}

public record ConnectionStrings
{
    public string PostgresDb { get; init; } = string.Empty;
    public bool IsValid() => !string.IsNullOrWhiteSpace(PostgresDb);
}

public record OpenAiConfiguration
{
    public string Endpoint { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public bool IsValid() => !string.IsNullOrWhiteSpace(Endpoint) &&
                             !string.IsNullOrWhiteSpace(Model) &&
                             !string.IsNullOrWhiteSpace(Key);
}

public record EmbeddingConfiguration
{
    public string Endpoint { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int Dimensions { get; init; } = 1536; // Default dimension for OpenAI embeddings

    public bool IsValid() => !string.IsNullOrWhiteSpace(Endpoint) &&
                             !string.IsNullOrWhiteSpace(Model) &&
                             !string.IsNullOrWhiteSpace(Key) &&
                             Dimensions > 0;
}

public record ChatSettings
{
    public int MaxRequestLength { get; init; } = 1000;
}
public record ApiSecurity
{
    public string ApiKey1 { get; init; } = string.Empty;
    public string ApiKey2 { get; init; } = string.Empty;
    public bool IsValid() => !string.IsNullOrWhiteSpace(ApiKey1) &&
                             !string.IsNullOrWhiteSpace(ApiKey2);
}