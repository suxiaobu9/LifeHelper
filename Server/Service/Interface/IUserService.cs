using LifeHelper.Shared.Models.LIFF;

namespace LifeHelper.Server.Service.Interface;

public interface IUserService
{

    /// <summary>
    /// 新增使用者
    /// </summary>
    /// <param name="userLineId"></param>
    /// <returns></returns>
    Task<User> AddUserAsync(string userLineId);

    /// <summary>
    /// 取得使用者
    /// </summary>
    /// <param name="userLineIds"></param>
    /// <returns></returns>
    Task<User[]> GetUsersAsync(string[] userLineIds);

    /// <summary>
    /// Upsert 目前使用者 並回傳
    /// </summary>
    /// <param name="userProfile"></param>
    /// <returns></returns>
    Task<User?> UpsertUserAsync(UserProfile userProfile);
}