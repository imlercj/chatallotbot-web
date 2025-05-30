using Chatallotbot.Server.Services;

namespace Chatallotbot.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddScoped<TestService>();
        return services;
    }
}