using isRock.LineBot;
using LifeHelper.Server.Models.LineApi;
using LifeHelper.Shared.Enum;

namespace LifeHelper.Server.Service;

public class LineBotApiService
{
    private readonly AccountingService accountingService;
    private readonly MemorandumService memorandumService;

    private delegate Task<LineReplyModel> EventProcess(string msg, int userId);

    public LineBotApiService(AccountingService accountingService,
        MemorandumService memorandumService)
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

        if (!string.IsNullOrWhiteSpace(msg))
        {
            // 判斷是記帳還是備忘錄
            EventProcess eventProcess = StringExtension.IsAccounting(msg) ? accountingService.AccountingAsync : memorandumService.RecordMemoAsync;
            var result = await eventProcess(msg, user.Id);
            return result;
        }

        return new LineReplyModel(LineReplyEnum.Message, "完成");
    }

}
