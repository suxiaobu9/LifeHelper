using isRock.LineBot;
using LifeHelper.Server.Models.Flex;
using LifeHelper.Server.Models.LineApi;
using LifeHelper.Server.Models.Template;
using LifeHelper.Shared.Enum;
using LifeHelper.Shared.Utility;
using System.Text;
using System.Text.RegularExpressions;

namespace LifeHelper.Server.Service;

public class LineBotApiService
{
    private readonly AccountingRepository accountingRepository;
    private readonly DeleteConfirmRepository deleteConfirmRepository;
    private readonly UnitOfWork unitOfWork;
    private readonly MemorandumRepository memorandumRepository;
    private readonly string IntRegex = @"-?\d+";

    public LineBotApiService(AccountingRepository accountingRepository,
        DeleteConfirmRepository deleteConfirmRepository,
        MemorandumRepository memorandumRepository,
        UnitOfWork unitOfWork)
    {
        this.accountingRepository = accountingRepository;
        this.deleteConfirmRepository = deleteConfirmRepository;
        this.unitOfWork = unitOfWork;
        this.memorandumRepository = memorandumRepository;
    }

    /// <summary>
    /// 處理訊息
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<LineReplyModel> AnalyzeMessages(Event lineEvent, User user)
    {
        var isAccounting = new Func<Event, bool>(x =>
        {
            var msg = lineEvent.message.text;
            var regexMatch = Regex.Match(msg, IntRegex);
            return regexMatch.Success && (msg.StartsWith(regexMatch.Value) || msg.EndsWith(regexMatch.Value));
        });

        EventProcess eventProcess;

        if (!string.IsNullOrWhiteSpace(lineEvent.message.text))
        {
            // 判斷是記帳還是備忘錄
            eventProcess = isAccounting(lineEvent) ? AddAccounting : AddMemo;
            var result = await eventProcess(lineEvent, user);
            return result;
        }

        return new LineReplyModel(LineReplyEnum.Message, "完成");
    }

    /// <summary>
    /// 確認刪除
    /// </summary>
    /// <param name="postbackData"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<LineReplyModel> AddDeleteConfirm(string postbackData, User user)
    {
        var utcNow = DateTime.UtcNow;
        var jsonParseSuccess = postbackData.JSONTryParse(out DeleteFeatureModel? model);

        if (!jsonParseSuccess || model == null)
            return new LineReplyModel(LineReplyEnum.Message, "資料錯誤");

        var (success, description) = await GetDescription(model.FeatureName, model.FeatureId, user);

        if (!success || string.IsNullOrWhiteSpace(description))
            return new LineReplyModel(LineReplyEnum.Message, "查無資料");

        var newDeleteConfirm = new DeleteConfirm
        {
            Deadline = utcNow.AddMinutes(5),
            UserId = user.Id,
            FeatureId = model.FeatureId,
            FeatureName = model.FeatureName
        };

        await deleteConfirmRepository.AddAsync(newDeleteConfirm);

        await unitOfWork.CompleteAsync();

        return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.DeleteComfirmFlexTemplate(new DeleteConfirmModel(newDeleteConfirm.Id, newDeleteConfirm.FeatureName, description)));
    }

    /// <summary>
    /// 更新過時時間
    /// </summary>
    /// <param name="deleteConfirm"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<LineReplyModel> UpdateDeadline(DeleteConfirm deleteConfirm, User user)
    {
        var utcNow = DateTime.UtcNow;
        deleteConfirm.Deadline = utcNow.AddMinutes(5);
        await unitOfWork.CompleteAsync();

        var (success, description) = await GetDescription(deleteConfirm.FeatureName, deleteConfirm.FeatureId, user);

        if (!success || string.IsNullOrWhiteSpace(description))
            return new LineReplyModel(LineReplyEnum.Message, "查無資料");

        return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.DeleteComfirmFlexTemplate(new DeleteConfirmModel(deleteConfirm.Id, deleteConfirm.FeatureName, description)));
    }

    /// <summary>
    /// 刪除資料
    /// </summary>
    /// <param name="deleteConfirm"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<LineReplyModel> KillData(DeleteConfirm deleteConfirm, User user)
    {
        switch (deleteConfirm.FeatureName)
        {
            case nameof(Models.EF.Accounting):
                var accounting = await accountingRepository.GetAccounting(deleteConfirm.FeatureId, user.LineUserId);
                if (accounting == null)
                    return new LineReplyModel(LineReplyEnum.Message, "查無資料");
                accountingRepository.Remove(accounting);
                break;
            case nameof(Models.EF.Memorandum):
                var memorandum = await memorandumRepository.GetMemorandum(deleteConfirm.FeatureId);
                if (memorandum == null)
                    return new LineReplyModel(LineReplyEnum.Message, "查無資料");
                memorandumRepository.Remove(memorandum);
                break;
            default:
                return new LineReplyModel(LineReplyEnum.Message, "查無資料");
        }

        await unitOfWork.CompleteAsync();

        return new LineReplyModel(LineReplyEnum.Message, "刪除成功");
    }

    /// <summary>
    /// Line 的 postback 動作
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <returns></returns>
    public async Task<LineReplyModel> Postback(Event lineEvent, User user)
    {
        var postbackData = Encoding.UTF8.GetString(Convert.FromBase64String(lineEvent.postback.data));

        // 取得確認刪除的 Id
        var getDeleteConfirmIdSuccess = int.TryParse(postbackData, out int deleteConfirmId);

        var utcNow = DateTime.UtcNow;

        // 沒有取得確認刪除的 Id ， 代表是需要確認的
        if (!getDeleteConfirmIdSuccess)
            return await AddDeleteConfirm(postbackData, user);

        var deleteConfirm = await deleteConfirmRepository.GetDeleteConfirm(deleteConfirmId);

        if (deleteConfirm == null)
            return new LineReplyModel(LineReplyEnum.Message, "查無資料");

        // 過期
        if (deleteConfirm.Deadline < utcNow)
            return await UpdateDeadline(deleteConfirm, user);

        //刪除資料
        return await KillData(deleteConfirm, user);
    }

    /// <summary>
    /// 找到 flex message 中的說明描述
    /// </summary>
    /// <param name="featureName"></param>
    /// <param name="featureId"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<(bool success, string? description)> GetDescription(string featureName, int featureId, User user)
    {
        var description = "";
        switch (featureName)
        {
            case nameof(Models.EF.Accounting):
                var accounting = await accountingRepository.GetAccounting(featureId, user.LineUserId);
                if (accounting == null)
                    return (false, null);
                description = accounting.Event;
                break;
            case nameof(Models.EF.Memorandum):
                var memorandum = await memorandumRepository.GetMemorandum(featureId);
                if (memorandum == null)
                    return (false, null);
                description = memorandum.Memo;
                break;
            default:
                return (false, null);
        }
        return (true, description);
    }

    private delegate Task<LineReplyModel> EventProcess(Event lineEvent, User user);

    /// <summary>
    /// 記帳
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<LineReplyModel> AddAccounting(Event lineEvent, User user)
    {
        var sourceMsg = lineEvent.message.text.Replace(Environment.NewLine, "");

        // 訊息中是否有整數
        var intRegex = Regex.Match(sourceMsg, IntRegex);

        if (!intRegex.Success)
            return new LineReplyModel(LineReplyEnum.Message, "取值錯誤");

        if (!int.TryParse(intRegex.Value, out int amount))
            return new LineReplyModel(LineReplyEnum.Message, "型別轉換錯誤");

        var eventName = sourceMsg.Replace(amount.ToString(), "").Replace("\n", "");

        if (string.IsNullOrWhiteSpace(eventName))
            eventName = "其他";

        // 記帳
        var utcNow = DateTime.UtcNow;

        var accounting = new Accounting
        {
            AccountDate = utcNow,
            CreateDate = utcNow,
            Amount = amount,
            UserId = user.Id,
            Event = eventName,
        };

        await accountingRepository.AddAsync(accounting);
        await unitOfWork.CompleteAsync();

        // 取得月帳務
        var monthlyAccountings = await accountingRepository.GetMonthlyAccounting(user.Id);

        var flexMessageModel = new AccountingFlexMessageModel
        {
            DeleteConfirm = new DeleteFeatureModel(nameof(Models.EF.Accounting), accounting.Id),
            MonthlyOutlay = monthlyAccountings.Where(x => x.Amount > 0).Sum(x => x.Amount),
            MonthlyIncome = Math.Abs(monthlyAccountings.Where(x => x.Amount < 0).Sum(x => x.Amount)),
            EventName = accounting.Event,
            Pay = amount,
            CreateDate = utcNow.AddHours(8)
        };

        return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.AccountingFlexMessageTemplate(flexMessageModel));
    }

    /// <summary>
    /// 備忘錄
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<LineReplyModel> AddMemo(Event lineEvent, User user)
    {
        await memorandumRepository.AddAsync(new Memorandum
        {
            Memo = lineEvent.message.text,
            UserId = user.Id
        });

        await unitOfWork.CompleteAsync();

        return new LineReplyModel(LineReplyEnum.Message, "備忘錄");
    }
}
