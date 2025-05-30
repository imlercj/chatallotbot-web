using Azure.Core;
using Azure.Identity;

namespace Chatallotbot.Server.Services;

public class MsiAuth(IConfiguration config, IHostEnvironment environment)
{
    public TokenCredential AzureCredentials { get; } = environment.IsDevelopment()
        ? new VisualStudioCredential(new VisualStudioCredentialOptions
        {
            TenantId = config["AzureResources:TenantId"]
        })
        : new ManagedIdentityCredential();
}