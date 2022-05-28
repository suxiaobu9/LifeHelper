using LifeHelper.Client.Provider;
using LifeHelper.Client.Service;
using Microsoft.AspNetCore.Components.Authorization;

namespace LifeHelper.Client.Extension;

public static class ServiceExtension
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<LIFFService>();
        services.AddScoped<AuthService>();

        services.AddScoped<AuthenticationStateProvider, LIFFAuthenticationProvider>();
        services.AddAuthorizationCore();
    }
}
