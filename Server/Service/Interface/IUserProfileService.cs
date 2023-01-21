using LifeHelper.Shared.Models.LIFF;

namespace LifeHelper.Server.Service.Interface;

public interface IUserProfileService
{

    /// <summary>
    /// 
    /// </summary>
    UserProfile? UserProfile { get; }

    /// <summary>
    /// 取得使用者資料
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<UserProfile?> GetUserProfileAsync(string token);

    /// <summary>
    /// 設定 User 資料
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task SetUserProfileAsync(HttpContext context);
}