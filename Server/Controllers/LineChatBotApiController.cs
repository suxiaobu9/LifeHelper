using isRock.LineBot;
using LifeHelper.Server.LineVerify;
using Microsoft.AspNetCore.Mvc;

namespace LifeHelper.Server.Controllers;

/// <summary>
/// ngrok http 5037 --host-header="localhost:5037"
/// https://developers.line.biz/console/channel/1657106184/liff/1657106184-R4A80Nrm
/// </summary>
[Route("LineChatBot")]

public class LineChatBotApiController : LineWebHookControllerBase
{
    [HttpPost]
    [Route("")]
    [LineVerifySignature]
    public IActionResult Index()
    {
        return Ok();
    }
}
