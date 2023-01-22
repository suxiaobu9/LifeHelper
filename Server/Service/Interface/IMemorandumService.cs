using LifeHelper.Server.Models.LineApi;

namespace LifeHelper.Server.Service.Interface;

public interface IMemorandumService
{

    /// <summary>
    /// 新增備註
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<Memorandum> AddMemoAsync(string msg, Guid userId);

    /// <summary>
    /// 取得使用者備忘錄
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<Memorandum[]> GetUserMemorandumAsync(Guid userId);

    /// <summary>
    /// 備忘錄
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<LineReplyModel> RecordMemoAsync(string msg, Guid userId);

    /// <summary>
    /// 刪除備忘錄
    /// </summary>
    /// <param name="memoId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task RemoveMemoAsync(Guid memoId, Guid userId);
}