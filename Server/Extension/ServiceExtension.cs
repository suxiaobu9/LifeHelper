namespace LifeHelper.Server.Extension;

public static class ServiceExtension
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<UserService>();
        services.AddScoped<DeleteAccountService>();
        services.AddScoped<LineBotApiService>();
        services.AddScoped<AccountingService>();
        services.AddScoped<UserProfileService>();
        
        services.AddScoped(typeof(Repository<>));
        services.AddScoped<UnitOfWork>();
        services.AddScoped<UserRepository>();
        services.AddScoped<DeleteAccountRepository>();
        services.AddScoped<AccountingRepository>();
        services.AddScoped<MemorandumRepository>();
        
        services.AddHttpClient();

    }
}
