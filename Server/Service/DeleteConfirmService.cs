using isRock.LineBot;
using LifeHelper.Server.Models.Flex;
using LifeHelper.Server.Models.LineApi;
using LifeHelper.Server.Models.Template;
using LifeHelper.Shared.Const;
using LifeHelper.Shared.Enum;

namespace LifeHelper.Server.Service;

public class DeleteConfirmService : IDeleteConfirmService
{
    private readonly AzureBlobStorageService azureBlobStorageService;
    private readonly IAccountingService accountingService;
    private readonly IMemorandumService memorandumService;

    public DeleteConfirmService(AzureBlobStorageService azureBlobStorageService,
        IAccountingService accountingService,
        IMemorandumService memorandumService)
    {
        this.azureBlobStorageService = azureBlobStorageService;
        this.accountingService = accountingService;
        this.memorandumService = memorandumService;
    }

    /// <summary>
    /// 新增刪除確認
    /// </summary>
    /// <param name="featureName"></param>
    /// <param name="featureId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<DeleteConfirm> AddDeleteConfirmAsync(string featureName, Guid featureId, Guid userId)
    {
        var deleteConfirm = new DeleteConfirm
        {
            Id = Guid.NewGuid(),
            FeatureName = featureName,
            FeatureId = featureId,
            UserId = userId,
            Deadline = DateTime.UtcNow.AddMinutes(5)
        };

        await azureBlobStorageService.UploadBlobAsync(BlobConst.DeleteConfirmBlobName(deleteConfirm.Id, userId, featureName), JsonSerializer.Serialize(deleteConfirm));
        return deleteConfirm;
    }

    /// <summary>
    /// 刪除資料
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <param name="user"></param>
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

        deleteConfirm = await azureBlobStorageService.GetBlob<DeleteConfirm>(BlobConst.DeleteConfirmBlobName(flexDeleteConfirm.Id.Value, user.Id, flexDeleteConfirm.FeatureName));

        if (deleteConfirm == null)
            return new LineReplyModel(LineReplyEnum.Message, "查無資料");

        // 過期
        if (deleteConfirm.Deadline < DateTime.UtcNow)
            return await UpdateDeadlineAsync(deleteConfirm, user);

        //刪除資料
        return await KillDataAsync(deleteConfirm, user.Id);

    }

    /// <summary>
    /// 找到 flex message 中的說明描述
    /// </summary>
    /// <param name="featureName"></param>
    /// <param name="featureId"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<string?> GetDescriptionAsync(string featureName, Guid featureId, User user)
    {
        var description = "";

        switch (featureName)
        {
            case nameof(Models.EF.Accounting):
                var accounting = await azureBlobStorageService.GetBlob<Accounting>(BlobConst.AccountingBlobName(user.Id, featureId));
                if (accounting == null)
                    return null;
                description = accounting.Event;
                break;
            case nameof(Models.EF.Memorandum):
                var memorandum = await azureBlobStorageService.GetBlob<Memorandum>(BlobConst.MemorandumBlobName(user.Id, featureId));
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

        await azureBlobStorageService.UploadBlobAsync(BlobConst.DeleteConfirmBlobName(deleteConfirm.Id, user.Id, deleteConfirm.FeatureName), JsonSerializer.Serialize(deleteConfirm));

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
    private async Task<LineReplyModel> KillDataAsync(DeleteConfirm deleteConfirm, Guid userId)
    {
        switch (deleteConfirm.FeatureName)
        {
            case nameof(Models.EF.Accounting):
                await accountingService.RemoveAccountingAsync(deleteConfirm.FeatureId, userId);
                // 取得月帳務
                var flexMessageModel = await accountingService.GetMonthlyAccountingAsync(userId);
                return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.AccountingFlexMessageTemplateAsync(flexMessageModel));
            case nameof(Models.EF.Memorandum):
                await memorandumService.RemoveMemoAsync(deleteConfirm.FeatureId, userId);
                var userMemoes = await memorandumService.GetUserMemorandumAsync(userId);
                return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.MemorandumFlexMessageTemplateAsync(userMemoes));
            default:
                return new LineReplyModel(LineReplyEnum.Message, "錯誤的功能");
        }
    }
}
