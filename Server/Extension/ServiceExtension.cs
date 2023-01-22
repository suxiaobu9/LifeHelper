
using LifeHelper.Server.Service;

namespace LifeHelper.Server.Extension;

public static class ServiceExtension
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDeleteConfirmService, DeleteConfirmService>();
        services.AddScoped<ILineBotApiService, LineBotApiService>();
        services.AddScoped<IAccountingService, AccountingService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IMemorandumService, MemorandumService>();
        services.AddScoped<AzureBlobStorageService>();

        services.AddHttpClient();

    }
}
