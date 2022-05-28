using Microsoft.EntityFrameworkCore;

namespace LifeHelper.Server.Repositories;

public class AccountingRepository : Repository<Accounting>
{
    public AccountingRepository(LifeHelperContext db) : base(db) { }

    /// <summary>
    /// 取得帳務資訊
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="lineUserId"></param>
    /// <returns></returns>
    public async Task<Accounting?> GetAccounting(int accountId, string lineUserId)
    {
        return await FirstOrDefaultAsync(x => x.Id == accountId && x.User.LineUserId == lineUserId);
    }

    /// <summary>
    /// 取得某月帳務
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Accounting[]> GetMonthlyAccounting(int userId, DateTime? utcDate = null)
    {
        utcDate ??= DateTime.UtcNow;

        var twNow = utcDate.Value.AddHours(8);
        var start = new DateTime(twNow.Year, twNow.Month, 1).ToUniversalTime();
        var end = new DateTime(twNow.Year, twNow.Month + 1, 1).AddMilliseconds(-1).ToUniversalTime();

        return await Where(x => start < x.AccountDate && x.AccountDate <= end && x.UserId == userId)
                        .AsNoTracking().ToArrayAsync();
    }
}
