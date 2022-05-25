using LifeHelper.Client.Service;

namespace LifeHelper.Client.Extension;

public static class ServiceExtension
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<LIFFService>();
    }
}
