using LifeHelper.Shared.Models.AppSettings;
using LifeHelper.Shared.Models.Login;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LifeHelper.Server.Controllers;

[Route("[controller]")]
public class LIFFController : Controller
{
    private readonly HttpClient httpClient;
    private readonly LIFFSetting LIFFSetting;
    private readonly UserProfileService userProfileService;
    private readonly LIFFSetting liff;

    public LIFFController(
        IOptions<LIFFSetting> LIFFSetting,
        UserProfileService userProfileService,
        HttpClient httpClient,
        IOptions<LIFFSetting> liff)
    {
        this.LIFFSetting = LIFFSetting.Value;
        this.userProfileService = userProfileService;
        this.httpClient = httpClient;
        this.liff = liff.Value;
    }

    /// <summary>
    /// 取得 LIFF id
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetLIFFId")]
    public string? GetLIFFId()
    {
        return LIFFSetting.LiffId;
    }

    /// <summary>
    /// 登入
    /// </summary>
    /// <param name="idToken"></param>
    /// <returns></returns>
    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync(string idToken)
    {
        var result = await userProfileService.GetUserProfileAsync(idToken);
        if (result == null)
            return new JsonResult(new { message = "Login failed" }) { StatusCode = StatusCodes.Status401Unauthorized };
        return Json(result);
    }

}
