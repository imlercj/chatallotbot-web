using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace Chatallotbot.Server.Extensions;

using Azure.Identity;

public static class ConfigurationExtensions
{
    public static void AddCustomConfiguration(this IConfigurationBuilder configuration, IWebHostEnvironment environment)
    {
        // Existing custom configuration logic
        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true);

        // Add Key Vault if configured
        var tempConfig = configuration.Build();
        var keyVaultUrl = tempConfig["AzureResources:KeyVaultUri"];
        if (string.IsNullOrEmpty(keyVaultUrl))
            throw new Exception("KeyVault not found");

        configuration.AddAzureKeyVault(
            new SecretClient(new Uri(keyVaultUrl), new ManagedIdentityCredential()),
            new KeyVaultSecretManager());
    }
}