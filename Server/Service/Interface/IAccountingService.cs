using LifeHelper.Server.Models.Flex;
using LifeHelper.Server.Models.LineApi;
using LifeHelper.Shared.Models.LIFF;

namespace LifeHelper.Server.Service.Interface;

public interface IAccountingService
{
    /// <summary>
    /// 記帳
    /// </summary>
    /// <param name="sourceMsg"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<LineReplyModel> AccountingAsync(string sourceMsg, int userId);

    /// <summary>
    /// 增加記帳
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="userId"></param>
    /// <param name="eventName"></param>
    /// <returns></returns>
    Task<Accounting> AddAccountingAsync(int amount, int userId, string eventName);

    /// <summary>
    /// 取得本月帳務
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<AccountingFlexMessageModel> GetMonthlyAccountingAsync(int userId);

    /// <summary>
    /// 取得月份帳務資料
    /// </summary>
    /// <param name="userProfile"></param>
    /// <param name="utcDate"></param>
    /// <returns></returns>
    Task<MonthlyAccountingVm?> MonthlyAccountingAsync(UserProfile userProfile, DateTime? utcDate);

    /// <summary>
    /// 刪除記帳
    /// </summary>
    /// <param name="accountingId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task RemoveAccountingAsync(int accountingId, int userId);
}