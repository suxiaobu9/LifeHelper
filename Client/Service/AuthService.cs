using Blazored.LocalStorage;
using LifeHelper.Client.Provider;
using LifeHelper.Shared.Models.LIFF;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;

namespace LifeHelper.Client.Service;

public class AuthService
{
    private readonly AuthenticationStateProvider authenticationStateProvider;
    private readonly LIFFService LIFF;
    private readonly HttpClient httpClient;
    private readonly ILocalStorageService localStorageService;

    public AuthService(AuthenticationStateProvider authenticationStateProvider,
        LIFFService LIFF,
        IHttpClientFactory httpClientFactory,
        ILocalStorageService localStorageService)
    {
        this.authenticationStateProvider = authenticationStateProvider;
        this.LIFF = LIFF;
        this.httpClient = httpClientFactory.CreateClient(nameof(HttpClient));
        this.localStorageService = localStorageService;
    }

    /// <summary>
    /// 登入
    /// </summary>
    /// <returns></returns>
    public async Task<bool> LoginAsync()
    {
        var idToken = await LIFF.GetIDTokenAsync();
        var dic = new Dictionary<string, string>
        {
            { "idToken" , idToken }
        };

        var response = await httpClient.PostAsync("LIFF/Login", new FormUrlEncodedContent(dic));

        if (!response.IsSuccessStatusCode)
            return false;

        try
        {
            var userProfile = JsonSerializer.Deserialize<UserProfile?>(await response.Content.ReadAsStringAsync());
            if (userProfile == null)
                return false;

            await localStorageService.SetItemAsync(nameof(UserProfile), userProfile);

        }
        catch (Exception ex)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 登出
    /// </summary>
    /// <returns></returns>
    public async Task<bool> LogoutAsync()
    {
        ((LIFFAuthenticationProvider)authenticationStateProvider).LogoutNotify();

        if (await localStorageService.ContainKeyAsync(nameof(UserProfile)))
            await localStorageService.RemoveItemAsync(nameof(UserProfile));

        return true;
    }
}
