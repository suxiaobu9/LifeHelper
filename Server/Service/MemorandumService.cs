using LifeHelper.Server.Models.LineApi;
using LifeHelper.Server.Models.Template;
using LifeHelper.Shared.Const;
using LifeHelper.Shared.Enum;

namespace LifeHelper.Server.Service;

public class MemorandumService : IMemorandumService
{

    private readonly AzureBlobStorageService azureBlobStorageService;

    public MemorandumService(AzureBlobStorageService azureBlobStorageService)
    {
        this.azureBlobStorageService = azureBlobStorageService;
    }

    /// <summary>
    /// 新增備註
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Memorandum> AddMemoAsync(string msg, Guid userId)
    {
        var memorandum = new Memorandum
        {
            Id = Guid.NewGuid(),
            Memo = msg,
            UserId = userId
        };

        await azureBlobStorageService.UploadBlobAsync(BlobConst.MemorandumBlobName(userId, memorandum.Id), JsonSerializer.Serialize(memorandum));

        return memorandum;
    }

    /// <summary>
    /// 取得使用者備忘錄
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Memorandum[]> GetUserMemorandumAsync(Guid userId)
    {
        return (await azureBlobStorageService.GetBlobs<Memorandum>(BlobConst.MemorandumBlobs(userId))).ToArray();
    }

    /// <summary>
    /// 備忘錄
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<LineReplyModel> RecordMemoAsync(string msg, Guid userId)
    {

        await AddMemoAsync(msg, userId);

        var userMemoes = await GetUserMemorandumAsync(userId);

        return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.MemorandumFlexMessageTemplateAsync(userMemoes));
    }

    /// <summary>
    /// 刪除備忘錄
    /// </summary>
    /// <param name="memoId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task RemoveMemoAsync(Guid memoId, Guid userId)
    {
        await azureBlobStorageService.DeleteBlob(BlobConst.MemorandumBlobName(userId, memoId));
    }
}
