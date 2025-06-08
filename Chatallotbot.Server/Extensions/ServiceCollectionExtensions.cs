using Chatallotbot.Server.Chat;
using Chatallotbot.Server.Services;
using Microsoft.AspNetCore.Authorization;

namespace Chatallotbot.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        /*services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());*/
        return services;
    }
    
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddScoped<MsiAuth>();
        services.AddScoped<ChatallotEmbeddingClient>();
        services.AddScoped<ChatallotChatClient>();
        services.AddScoped<IChatService, ChatService>();
        return services;
    }
}