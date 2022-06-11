using Blazored.LocalStorage;
using LifeHelper.Client;
using LifeHelper.Client.Extension;
using LifeHelper.Client.Handler;
using LifeHelper.Shared.Models.LIFF;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Security.Claims;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddServices();

var localStorageService = builder.Services.BuildServiceProvider().GetService<ILocalStorageService>();

var userProfile = localStorageService == null ? null : await localStorageService.GetItemAsync<UserProfile>(nameof(UserProfile));

builder.Services.AddHttpClient(nameof(HttpClient),
    client =>
    {
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);

        if (userProfile != null)
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {userProfile.IdToken}");

    }).AddHttpMessageHandler<UnauthorizedResponseHandler>();


await builder.Build().RunAsync();
