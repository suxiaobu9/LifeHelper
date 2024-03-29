﻿using isRock.LineBot;
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
    private readonly IUserService userService;
    private readonly IDeleteConfirmService deleteConfirmService;
    private readonly ILineBotApiService lineBotApiService;

    public LineChatBotApiController(IUserService userService,
        ILineBotApiService lineBotApiService,
        IDeleteConfirmService deleteConfirmService,
        IOptions<LineChatBotSetting> lineChatBotSetting)
    {
        this.userService = userService;
        this.lineBotApiService = lineBotApiService;
        this.ChannelAccessToken = lineChatBotSetting.Value.ChannelAccessToken;
        this.deleteConfirmService = deleteConfirmService;
    }

    /// <summary>
    /// 用來接 Line Webhook
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route("")]
    [LineVerifySignature]
    public async Task<IActionResult> IndexAsync()
    {
        try
        {
            if (this.ReceivedMessage == null ||
                this.ReceivedMessage.events == null ||
                !this.ReceivedMessage.events.Any())
                return Ok();

            var isPostbackEvent = new Func<Event, bool>(x =>
            {
                return x.type.ToLower() == "postback";
            });

            var isTextMessageEvent = new Func<Event, bool>(x =>
            {
                return x.type.ToLower() == "message" && x.message.type == "text";
            });

            var lineEvents = this.ReceivedMessage.events
                .Where(x => x != null && (isPostbackEvent(x) || isTextMessageEvent(x)));

            var allUserLineIds = lineEvents.Select(x => x.source.userId)
                                                .Distinct().ToArray();
            foreach (var lineEvent in lineEvents)
            {
                var user = await userService.GetUserAsync(lineEvent.source.userId);

                if (user == null)
                    user = await userService.AddUserAsync(lineEvent.source.userId);

                if (isPostbackEvent(lineEvent))
                {
                    var confirmResult = await deleteConfirmService.DeleteConfirmationAsync(lineEvent, user);

                    ReplyToUser(lineEvent, confirmResult);

                    continue;
                }

                var analyzeMessagesResult = await lineBotApiService.AnalyzeMessagesAsync(lineEvent, user);

                ReplyToUser(lineEvent, analyzeMessagesResult);
            }

        }
        catch
        {
            throw;
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
            LineReplyEnum.Json => PushMessagesWithJSON,
            _ => null,
        };

        if (lineReply == null)
            return;

        lineReply(lineEvent.source.userId, model.Message);
    }
}
