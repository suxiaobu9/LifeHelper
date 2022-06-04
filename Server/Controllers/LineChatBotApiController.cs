using isRock.LineBot;
using LifeHelper.Server.Attributes.LineVerify;
using LifeHelper.Server.Models.LineApi;
using LifeHelper.Shared.Enum;
using LifeHelper.Shared.Models.AppSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LifeHelper.Server.Controllers;

/// <summary>
/// ngrok http 5037 --host-header="localhost:5037"
/// https://developers.line.biz/console/channel/1657106184/liff/1657106184-R4A80Nrm
/// </summary>
[Route("LineChatBot")]

public class LineChatBotApiController : LineWebHookControllerBase
{
    private readonly UserService userService;
    private readonly LineBotApiService lineBotApiService;

    public LineChatBotApiController(UserService userService,
        LineBotApiService lineBotApiService,
        IOptions<LineChatBotSetting> lineChatBotSetting)
    {
        this.userService = userService;
        this.lineBotApiService = lineBotApiService;
        this.ChannelAccessToken = lineChatBotSetting.Value.ChannelAccessToken;
    }

    /// <summary>
    /// 用來接 Line Webhook
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route("")]
    [LineVerifySignature]
    public async Task<IActionResult> Index()
    {
        try
        {
            if (this.ReceivedMessage == null || 
                this.ReceivedMessage.events == null || 
                !this.ReceivedMessage.events.Any())
                return Ok();

            var isPostback = new Func<Event, bool>(x =>
            {
                return x.type.ToLower() == "postback";
            });

            var isText = new Func<Event, bool>(x =>
            {
                return x.type.ToLower() == "message" && x.message.type == "text";
            });

            var lineEvents = this.ReceivedMessage.events
                .Where(x => x != null && (isPostback(x) || isText(x)));

            var allEventUserIds = lineEvents.Select(x => x.source.userId)
                                                .Distinct().ToArray();

            var allUsers = await userService.GetUsers(allEventUserIds);

            foreach (var item in lineEvents)
            {
                var user = allUsers.FirstOrDefault(x => x.LineUserId == item.source.userId);

                if (user == null)
                    user = await userService.AddUser(item.source.userId);

                if (isPostback(item))
                {
                    var postBackResult = await lineBotApiService.Postback(item, user);

                    ReplyToUser(item, postBackResult);

                    continue;
                }

                var analyzeMessagesResult = await lineBotApiService.AnalyzeMessages(item, user);

                ReplyToUser(item, analyzeMessagesResult);
            }

        }
        catch (Exception ex)
        {
        }
        return Ok();
    }

    private delegate string LineReply(string replyToken, string message);

    /// <summary>
    /// 回應 User
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <param name="model"></param>
    private void ReplyToUser(Event lineEvent, LineReplyModel model)
    {

        LineReply? lineReply = model.LineReplyType switch
        {
            LineReplyEnum.Message => ReplyMessage,
            LineReplyEnum.Json => ReplyMessageWithJSON,
            _ => null,
        };

        if (lineReply == null)
            return;

        lineReply(lineEvent.replyToken, model.Message);
    }
}
