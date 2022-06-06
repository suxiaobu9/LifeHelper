namespace LifeHelper.Server.Extension;

public static class ServiceExtension
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<UserService>();
        services.AddScoped<DeleteConfirmService>();
        services.AddScoped<LineBotApiService>();
        services.AddScoped<AccountingService>();
        services.AddScoped<UserProfileService>();
        services.AddScoped<MemorandumService>();

        services.AddScoped(typeof(Repository<>));
        services.AddScoped<UnitOfWork>();
        services.AddScoped<UserRepository>();
        services.AddScoped<DeleteConfirmRepository>();
        services.AddScoped<AccountingRepository>();
        services.AddScoped<MemorandumRepository>();

        services.AddHttpClient();

    }
}
