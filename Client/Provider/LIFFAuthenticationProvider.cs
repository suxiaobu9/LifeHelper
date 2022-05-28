using LifeHelper.Shared.Models.LIFF;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace LifeHelper.Client.Provider;

public class LIFFAuthenticationProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal ClaimsPrincipal = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await Task.FromResult(0);
        return new AuthenticationState(ClaimsPrincipal);
    }

    /// <summary>
    /// 登入通知系統
    /// </summary>
    /// <param name="userProfile"></param>
    public void LoginNotify(UserProfile userProfile)
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, userProfile.Name),
            }, "Liff Authentication");
        ClaimsPrincipal = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>
    /// 登出通知系統
    /// </summary>
    public void LogoutNotify()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        ClaimsPrincipal = anonymous;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    }

}
