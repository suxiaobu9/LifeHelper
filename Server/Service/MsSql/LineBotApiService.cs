using isRock.LineBot;
using LifeHelper.Server.Models.LineApi;
using LifeHelper.Server.Models.Template;
using LifeHelper.Server.Service.Interface;
using LifeHelper.Shared.Enum;

namespace LifeHelper.Server.Service.MsSql;

public class LineBotApiService : ILineBotApiService
{
    private readonly IAccountingService accountingService;
    private readonly IMemorandumService memorandumService;

    private delegate Task<LineReplyModel> EventProcess(string msg, int userId);

    public LineBotApiService(IAccountingService accountingService,
        IMemorandumService memorandumService)
    {
        this.accountingService = accountingService;
        this.memorandumService = memorandumService;
    }

    /// <summary>
    /// 處理訊息
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<LineReplyModel> AnalyzeMessagesAsync(Event lineEvent, User user)
    {
        var msg = lineEvent.message.text;

        if (string.IsNullOrWhiteSpace(msg))
            return new LineReplyModel(LineReplyEnum.Message, "完成");

        if (msg == "/記帳")
        {
            var accountingData = await accountingService.GetMonthlyAccountingAsync(user.Id);
            return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.AccountingFlexMessageTemplateAsync(accountingData));
        }

        if (msg == "/備忘錄")
        {
            var userMemoes = await memorandumService.GetUserMemorandumAsync(user.Id);
            return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.MemorandumFlexMessageTemplateAsync(userMemoes));
        }

        // 判斷是記帳還是備忘錄
        EventProcess eventProcess = StringExtension.IsAccounting(msg) ? accountingService.AccountingAsync : memorandumService.RecordMemoAsync;
        var result = await eventProcess(msg, user.Id);
        return result;

    }

}
