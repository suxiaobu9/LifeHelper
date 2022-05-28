using LifeHelper.Client.Provider;
using LifeHelper.Shared.Models.LIFF;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;

namespace LifeHelper.Client.Service;

public class AuthService
{
    private readonly AuthenticationStateProvider authenticationStateProvider;
    private readonly LIFFService LIFF;
    private HttpClient httpClient;

    public AuthService(AuthenticationStateProvider authenticationStateProvider,
        LIFFService LIFF,
        HttpClient httpClient)
    {
        this.authenticationStateProvider = authenticationStateProvider;
        this.LIFF = LIFF;
        this.httpClient = httpClient;
    }

    /// <summary>
    /// 登入
    /// </summary>
    /// <returns></returns>
    public async Task<bool> Login()
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

            ((LIFFAuthenticationProvider)authenticationStateProvider).LoginNotify(userProfile);

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {idToken}");
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
    public bool Logout()
    {
        ((LIFFAuthenticationProvider)authenticationStateProvider).LogoutNotify();
        return true;
    }
}
