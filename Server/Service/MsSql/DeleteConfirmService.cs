using isRock.LineBot;
using LifeHelper.Server.Models.Flex;
using LifeHelper.Server.Models.LineApi;
using LifeHelper.Server.Models.Template;
using LifeHelper.Server.Service.Interface;
using LifeHelper.Shared.Enum;

namespace LifeHelper.Server.Service.MsSql;

public class DeleteConfirmService : IDeleteConfirmService
{
    private readonly DeleteConfirmRepository deleteConfirmRepository;
    private readonly UnitOfWork unitOfWork;
    private readonly AccountingRepository accountingRepository;
    private readonly IAccountingService accountingService;
    private readonly MemorandumRepository memorandumRepository;
    private readonly IMemorandumService memorandumService;
    public DeleteConfirmService(DeleteConfirmRepository deleteAccountRepository,
        UnitOfWork unitOfWork,
        AccountingRepository accountingRepository,
        MemorandumRepository memorandumRepository,
        IAccountingService accountingService,
        IMemorandumService memorandumService)
    {
        deleteConfirmRepository = deleteAccountRepository;
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
    public Task<DeleteConfirm?> GetDeleteConfirmAsync(int accountId)
    {
        return deleteConfirmRepository.GetDeleteConfirmAsync(accountId);
    }

    /// <summary>
    /// 刪除資料
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <returns></returns>
    public async Task<LineReplyModel> DeleteConfirmationAsync(Event lineEvent, User user)
    {
        var postbackData = lineEvent.postback.data.Base64Decode();

        var convertJsonSuccess = postbackData.JSONTryParse(out FlexDeleteConfirmModel? flexDeleteConfirm);

        // 轉換成 JSON 或是 model 失敗
        if (!convertJsonSuccess || flexDeleteConfirm == null)
            return new LineReplyModel(LineReplyEnum.Message, "格式錯誤");

        var description = await GetDescriptionAsync(flexDeleteConfirm.FeatureName, flexDeleteConfirm.FeatureId, user);

        if (string.IsNullOrWhiteSpace(description))
            return new LineReplyModel(LineReplyEnum.Message, "查無資料");

        DeleteConfirm? deleteConfirm;

        if (flexDeleteConfirm.Id == null)
        {
            deleteConfirm = await AddDeleteConfirmAsync(flexDeleteConfirm.FeatureName, flexDeleteConfirm.FeatureId, user.Id);
            return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.DeleteComfirmFlexTemplateAsync(description, new FlexDeleteConfirmModel(deleteConfirm.Id, deleteConfirm.FeatureName, deleteConfirm.FeatureId)));
        }

        deleteConfirm = await deleteConfirmRepository.GetDeleteConfirmAsync(flexDeleteConfirm.Id.Value);

        if (deleteConfirm == null)
            return new LineReplyModel(LineReplyEnum.Message, "查無資料");

        // 過期
        if (deleteConfirm.Deadline < DateTime.UtcNow)
            return await UpdateDeadlineAsync(deleteConfirm, user);

        //刪除資料
        return await KillDataAsync(deleteConfirm, user.Id);
    }

    /// <summary>
    /// 新增刪除確認
    /// </summary>
    /// <param name="featureName"></param>
    /// <param name="featureId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<DeleteConfirm> AddDeleteConfirmAsync(string featureName, int featureId, int userId)
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
    private async Task<string?> GetDescriptionAsync(string featureName, int featureId, User user)
    {
        var description = "";

        switch (featureName)
        {
            case nameof(Accounting):
                var accounting = await accountingRepository.GetAccountingAsync(featureId, user.Id);
                if (accounting == null)
                    return null;
                description = accounting.Event;
                break;
            case nameof(Memorandum):
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
    private async Task<LineReplyModel> UpdateDeadlineAsync(DeleteConfirm deleteConfirm, User user)
    {
        var utcNow = DateTime.UtcNow;
        deleteConfirm.Deadline = utcNow.AddMinutes(5);
        await unitOfWork.CompleteAsync();

        var description = await GetDescriptionAsync(deleteConfirm.FeatureName, deleteConfirm.FeatureId, user);

        if (string.IsNullOrWhiteSpace(description))
            return new LineReplyModel(LineReplyEnum.Message, "查無資料");

        return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.DeleteComfirmFlexTemplateAsync(description, new FlexDeleteConfirmModel(deleteConfirm.Id, deleteConfirm.FeatureName, deleteConfirm.FeatureId)));
    }

    /// <summary>
    /// 刪除資料
    /// </summary>
    /// <param name="deleteConfirm"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<LineReplyModel> KillDataAsync(DeleteConfirm deleteConfirm, int userId)
    {
        switch (deleteConfirm.FeatureName)
        {
            case nameof(Accounting):
                await accountingService.RemoveAccountingAsync(deleteConfirm.FeatureId, userId);
                // 取得月帳務
                var flexMessageModel = await accountingService.GetMonthlyAccountingAsync(userId);
                return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.AccountingFlexMessageTemplateAsync(flexMessageModel));
            case nameof(Memorandum):
                await memorandumService.RemoveMemoAsync(deleteConfirm.FeatureId, userId);
                var userMemoes = await memorandumService.GetUserMemorandumAsync(userId);
                return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.MemorandumFlexMessageTemplateAsync(userMemoes));
            default:
                return new LineReplyModel(LineReplyEnum.Message, "錯誤的功能");
        }
    }
}
