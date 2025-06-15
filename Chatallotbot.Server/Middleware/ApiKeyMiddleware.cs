using System.Security.Cryptography;
using Chatallotbot.Server.Configuration;

namespace Chatallotbot.Server.Middleware;

public class ApiKeyMiddleware(RequestDelegate next)
{
    private const string ApiKeyHeaderName = "X-Chatallot-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey) ||
            string.IsNullOrWhiteSpace(extractedApiKey))
        {
            // If the API key is not present in the headers, return 401 Unauthorized
            // This is a security measure to ensure that all requests have a valid API key
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("API Key is missing");
            return;
        }

        // Get the domain from the request
        var domain = context.Request.Host.Host;

        // Generate the expected key for this domain
        var expectedKey = GenerateApiKey(domain, AppConfig.ApiSecurity.ApiKey1);
        if (!extractedApiKey.Equals(expectedKey) &&
            !extractedApiKey.Equals(GenerateApiKey(domain, AppConfig.ApiSecurity.ApiKey2)))
        {
            // If the key doesn't match, try the second predefined key
            // This allows for a fallback or alternative key
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        await next(context);
    }

    private static string GenerateApiKey(string domainName, string predefinedKey)
    {
        // Combine domain name and predefined key
        var combined = $"{domainName}:{predefinedKey}";

        // Create a SHA256 hash
        var hashBytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(combined));

        // Convert to Base64 string for a more API key friendly format
        return Convert.ToBase64String(hashBytes)
            .Replace("/", "_")
            .Replace("+", "-")
            .Replace("=", "");
    }
}