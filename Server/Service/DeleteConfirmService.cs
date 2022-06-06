using isRock.LineBot;
using LifeHelper.Server.Models.Flex;
using LifeHelper.Server.Models.LineApi;
using LifeHelper.Server.Models.Template;
using LifeHelper.Shared.Enum;

namespace LifeHelper.Server.Service;

public class DeleteConfirmService
{
    private readonly DeleteConfirmRepository deleteConfirmRepository;
    private readonly UnitOfWork unitOfWork;
    private readonly AccountingRepository accountingRepository;
    private readonly AccountingService accountingService;
    private readonly MemorandumRepository memorandumRepository;
    private readonly MemorandumService memorandumService;
    public DeleteConfirmService(DeleteConfirmRepository deleteAccountRepository,
        UnitOfWork unitOfWork,
        AccountingRepository accountingRepository,
        MemorandumRepository memorandumRepository,
        AccountingService accountingService,
        MemorandumService memorandumService)
    {
        this.deleteConfirmRepository = deleteAccountRepository;
        this.unitOfWork = unitOfWork;
        this.accountingRepository = accountingRepository;
        this.memorandumRepository = memorandumRepository;
        this.accountingService = accountingService;
        this.memorandumService = memorandumService;
    }

    /// <summary>
    /// 取得刪除帳務的資料
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    public async Task<DeleteConfirm?> GetDeleteConfirm(int accountId)
    {
        return await deleteConfirmRepository.GetDeleteConfirm(accountId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <returns></returns>
    public async Task<LineReplyModel> DeleteConfirmation(Event lineEvent, User user)
    {
        var postbackData = lineEvent.postback.data.Base64Decode();

        var convertJsonSuccess = postbackData.JSONTryParse(out FlexDeleteConfirmModel? flexDeleteConfirm);

        // 轉換成 JSON 或是 model 失敗
        if (!convertJsonSuccess || flexDeleteConfirm == null)
            return new LineReplyModel(LineReplyEnum.Message, "格式錯誤");

        var description = await GetDescription(flexDeleteConfirm.FeatureName, flexDeleteConfirm.FeatureId, user);

        if (string.IsNullOrWhiteSpace(description))
            return new LineReplyModel(LineReplyEnum.Message, "查無資料");

        DeleteConfirm? deleteConfirm;

        if (flexDeleteConfirm.Id == null)
        {
            deleteConfirm = await AddDeleteConfirm(flexDeleteConfirm.FeatureName, flexDeleteConfirm.FeatureId, user.Id);
            return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.DeleteComfirmFlexTemplate(description, new FlexDeleteConfirmModel(deleteConfirm.Id, deleteConfirm.FeatureName, deleteConfirm.FeatureId)));
        }

        deleteConfirm = await deleteConfirmRepository.GetDeleteConfirm(flexDeleteConfirm.Id.Value);

        if (deleteConfirm == null)
            return new LineReplyModel(LineReplyEnum.Message, "查無資料");

        // 過期
        if (deleteConfirm.Deadline < DateTime.UtcNow)
            return await UpdateDeadline(deleteConfirm, user);

        //刪除資料
        return await KillData(deleteConfirm, user.Id);
    }

    /// <summary>
    /// 新增刪除確認
    /// </summary>
    /// <param name="featureName"></param>
    /// <param name="featureId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<DeleteConfirm> AddDeleteConfirm(string featureName, int featureId, int userId)
    {
        var deleteConfirm = new DeleteConfirm
        {
            FeatureName = featureName,
            FeatureId = featureId,
            UserId = userId,
            Deadline = DateTime.UtcNow.AddMinutes(5)
        };

        await deleteConfirmRepository.AddAsync(deleteConfirm);

        await unitOfWork.CompleteAsync();

        return deleteConfirm;
    }

    /// <summary>
    /// 找到 flex message 中的說明描述
    /// </summary>
    /// <param name="featureName"></param>
    /// <param name="featureId"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<string?> GetDescription(string featureName, int featureId, User user)
    {
        var description = "";

        switch (featureName)
        {
            case nameof(Models.EF.Accounting):
                var accounting = await accountingRepository.GetAccounting(featureId, user.Id);
                if (accounting == null)
                    return null;
                description = accounting.Event;
                break;
            case nameof(Models.EF.Memorandum):
                var memorandum = await memorandumRepository.GetMemorandum(featureId, user.Id);
                if (memorandum == null)
                    return null;
                description = memorandum.Memo;
                break;
            default:
                return null;
        }

        return description;
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

        var description = await GetDescription(deleteConfirm.FeatureName, deleteConfirm.FeatureId, user);

        if (string.IsNullOrWhiteSpace(description))
            return new LineReplyModel(LineReplyEnum.Message, "查無資料");

        return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.DeleteComfirmFlexTemplate(description, new FlexDeleteConfirmModel(deleteConfirm.Id, deleteConfirm.FeatureName, deleteConfirm.FeatureId)));
    }

    /// <summary>
    /// 刪除資料
    /// </summary>
    /// <param name="deleteConfirm"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<LineReplyModel> KillData(DeleteConfirm deleteConfirm, int userId)
    {
        switch (deleteConfirm.FeatureName)
        {
            case nameof(Models.EF.Accounting):
                await accountingService.RemoveAccounting(deleteConfirm.FeatureId, userId);
                // 取得月帳務
                var monthlyAccountings = await accountingRepository.GetMonthlyAccounting(userId);
                var flexMessageModel = new AccountingFlexMessageModel
                {
                    MonthlyOutlay = monthlyAccountings.Where(x => x.Amount > 0).Sum(x => x.Amount),
                    MonthlyIncome = Math.Abs(monthlyAccountings.Where(x => x.Amount < 0).Sum(x => x.Amount)),
                    CreateDate = DateTime.UtcNow.AddHours(8),
                };
                return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.AccountingFlexMessageTemplate(flexMessageModel));
            case nameof(Models.EF.Memorandum):
                await memorandumService.RemoveMemo(deleteConfirm.FeatureId, userId);
                var userMemoes = await memorandumRepository.GetUserMemorandum(userId);
                return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.MemorandumFlexMessageTemplate(userMemoes));
            default:
                return new LineReplyModel(LineReplyEnum.Message, "錯誤的功能");
        }
    }
}
