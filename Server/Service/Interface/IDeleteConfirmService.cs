using isRock.LineBot;
using LifeHelper.Server.Models.LineApi;

namespace LifeHelper.Server.Service.Interface;

public interface IDeleteConfirmService
{
    /// <summary>
    /// 新增刪除確認
    /// </summary>
    /// <param name="featureName"></param>
    /// <param name="featureId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<DeleteConfirm> AddDeleteConfirmAsync(string featureName, int featureId, int userId);

    /// <summary>
    /// 刪除資料
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<LineReplyModel> DeleteConfirmationAsync(Event lineEvent, User user);

    /// <summary>
    /// 取得刪除帳務的資料
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    Task<DeleteConfirm?> GetDeleteConfirmAsync(int accountId);
}