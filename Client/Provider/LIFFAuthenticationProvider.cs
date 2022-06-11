using Blazored.LocalStorage;
using LifeHelper.Shared.Models.LIFF;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace LifeHelper.Client.Provider;

public class LIFFAuthenticationProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal ClaimsPrincipal = new(new ClaimsIdentity());
    private readonly ILocalStorageService localStorageService;
    private readonly HttpClient httpClient;

    public LIFFAuthenticationProvider(ILocalStorageService localStorageService,
        IHttpClientFactory httpClientFactory)
    {
        this.localStorageService = localStorageService;
        this.httpClient = httpClientFactory.CreateClient(nameof(HttpClient));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await Task.FromResult(0);
        return new AuthenticationState(ClaimsPrincipal);
    }

    /// <summary>
    /// 登入通知系統
    /// </summary>
    /// <param name="userProfile"></param>
    public async Task LoginNotifyAsync()
    {
        var userProfile = await localStorageService.GetItemAsync<UserProfile>(nameof(UserProfile));
        if (userProfile != null)
        {
            var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, userProfile.Name),
                }, "Liff Authentication");
            ClaimsPrincipal = new ClaimsPrincipal(identity);
        }

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>
    /// 登出通知系統
    /// </summary>
    public void LogoutNotify()
    {

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    }

}
