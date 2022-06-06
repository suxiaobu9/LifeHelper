using isRock.LineBot;
using LifeHelper.Server.Models.Flex;
using LifeHelper.Server.Models.LineApi;
using LifeHelper.Server.Models.Template;
using LifeHelper.Shared.Const;
using LifeHelper.Shared.Enum;
using LifeHelper.Shared.Utility;
using System.Text;
using System.Text.RegularExpressions;

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
    public async Task<LineReplyModel> AnalyzeMessages(Event lineEvent, User user)
    {
        var msg = lineEvent.message.text;

        var isAccounting = new Func<bool>(() =>
        {
            var regexMatch = Regex.Match(msg, RegexConst.IntRegex);
            return regexMatch.Success && (msg.StartsWith(regexMatch.Value) || msg.EndsWith(regexMatch.Value));
        });

        if (!string.IsNullOrWhiteSpace(msg))
        {
            // 判斷是記帳還是備忘錄
            EventProcess eventProcess = isAccounting() ? accountingService.Accounting : memorandumService.RecordMemo;
            var result = await eventProcess(msg, user.Id);
            return result;
        }

        return new LineReplyModel(LineReplyEnum.Message, "完成");
    }

}
