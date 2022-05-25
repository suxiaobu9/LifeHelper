using LifeHelper.Shared.Models.AppSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LifeHelper.Server.Controllers;

[Route("[controller]")]
public class LIFFController : Controller
{

    private readonly LIFFSetting LIFFSetting;

    public LIFFController(IOptions<LIFFSetting> LIFFSetting)
    {
        this.LIFFSetting = LIFFSetting.Value;
    }

    [HttpGet("GetLIFFId")]
    public string? GetLIFFId()
    {
        return LIFFSetting.LiffId;
    }
}
