using isRock.LineBot;
using LifeHelper.Server.Models.LineApi;

namespace LifeHelper.Server.Service.Interface;

public interface ILineBotApiService
{
    /// <summary>
    /// 處理訊息
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<LineReplyModel> AnalyzeMessagesAsync(Event lineEvent, User user);
}