using LifeHelper.Shared.Models.AppSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LifeHelper.Server.Controllers;

[Route("[controller]")]
public class LIFFController : Controller
{

    private readonly LIFFSetting LIFFSetting;
    private readonly UserProfileService userProfileService;

    public LIFFController(IOptions<LIFFSetting> LIFFSetting,
        UserProfileService userProfileService)
    {
        this.LIFFSetting = LIFFSetting.Value;
        this.userProfileService = userProfileService;
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
    public async Task<IActionResult> Login(string idToken)
    {
        var result = await userProfileService.GetUserProfile(idToken);
        if (result == null)
            return new JsonResult(new { message = "Login failed" }) { StatusCode = StatusCodes.Status401Unauthorized };
        return Json(result);
    }

}
