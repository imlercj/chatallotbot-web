using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Chatallotbot.Server.Configuration;
using Chatallotbot.Server.Services;

namespace Chatallotbot.Server.Extensions;

using Azure.Identity;

public static class ConfigurationExtensions
{
    public static void AddCustomConfiguration(this ConfigurationManager configuration, IWebHostEnvironment environment)
    {
        // Existing custom configuration logic
        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true);

        // Add Key Vault if configured
        var keyVaultUrl = configuration["AzureResources:KeyVaultUri"];
        if (string.IsNullOrEmpty(keyVaultUrl))
            throw new Exception("KeyVault not found");

      
        var msiAuth = new MsiAuth(configuration, environment);
        configuration.AddAzureKeyVault(
            new SecretClient(new Uri(keyVaultUrl), msiAuth.AzureCredentials),
            new KeyVaultSecretManager());
        
        AppConfig.Initialize(configuration, environment);
    }
}