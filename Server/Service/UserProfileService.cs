using LifeHelper.Shared.Models.AppSettings;
using LifeHelper.Shared.Models.LIFF;
using Microsoft.Extensions.Options;

namespace LifeHelper.Server.Service;

public class UserProfileService
{
    public UserProfile? UserProfile { get; private set; }
    private readonly LIFFSetting LIFFSetting;
    private readonly HttpClient httpClient;

    public UserProfileService(IOptions<LIFFSetting> LIFFSetting,
        HttpClient http)
    {
        this.LIFFSetting = LIFFSetting.Value;
        this.httpClient = http;
    }

    /// <summary>
    /// 取得使用者資料
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<UserProfile?> GetUserProfileAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(LIFFSetting.ChannelId))
            return null;

        var dict = new Dictionary<string, string>
        {
            { "id_token", token },
            { "client_id", LIFFSetting.ChannelId }
        };

        var responseMsg = await httpClient.PostAsync("https://api.line.me/oauth2/v2.1/verify", new FormUrlEncodedContent(dict));

        if (!responseMsg.IsSuccessStatusCode)
            return null;

        var result = await responseMsg.Content.ReadFromJsonAsync<UserProfile?>();

        if (result == null)
            return null;

        result.IdToken = token;

        return result;
    }

    /// <summary>
    /// 設定 User 資料
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task SetUserProfileAsync(HttpContext context)
    {
        var authorization = context.Request.Headers["Authorization"];

        if (authorization.Count == 0)
            return;

        var token = authorization[0]["bearer".Length..];


        UserProfile = await GetUserProfileAsync(token);
    }
}
