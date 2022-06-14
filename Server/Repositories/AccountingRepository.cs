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
    public Task<Accounting?> GetAccountingAsync(int accountId, string lineUserId)
    {
        return FirstOrDefaultAsync(x => x.Id == accountId && x.User.LineUserId == lineUserId);
    }

    /// <summary>
    /// 取得帳務資訊
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<Accounting?> GetAccountingAsync(int accountId, int userId)
    {
        return FirstOrDefaultAsync(x => x.Id == accountId && x.UserId == userId);
    }

    /// <summary>
    /// 取得某月帳務
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<Accounting[]> GetMonthlyAccountingAsync(int userId, DateTime utcDate)
    {
        var twDate = utcDate.AddHours(8);
        var start = new DateTime(twDate.Year, twDate.Month, 1).ToUniversalTime();

        twDate = twDate.AddMonths(1);

        var end = new DateTime(twDate.Year, twDate.Month, 1).AddMilliseconds(-1).ToUniversalTime();

        return Where(x => start <= x.AccountDate && x.AccountDate <= end && x.UserId == userId)
                        .AsNoTracking().ToArrayAsync();
    }

    /// <summary>
    /// 取得上個月分
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="utcDate"></param>
    /// <returns></returns>
    public async Task<DateTime?> GetPreAccouningUtcDateAsync(int userId, DateTime utcDate)
    {
        var twDate = utcDate.AddHours(8);
        var maxDate = new DateTime(twDate.Year, twDate.Month, 1).ToUniversalTime();

        var data = await Where(x => x.AccountDate < maxDate && x.UserId == userId)
            .OrderByDescending(x => x.AccountDate)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return data?.AccountDate;
    }

    /// <summary>
    /// 取得下個月份
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="utcDate"></param>
    /// <returns></returns>
    public async Task<DateTime?> GetNextAccountingUtcDateAsync(int userId, DateTime utcDate)
    {
        var twDate = utcDate.AddHours(8).AddMonths(1);
        var minDate = new DateTime(twDate.Year, twDate.Month, 1).AddMilliseconds(-1).ToUniversalTime();

        var data = await Where(x => x.AccountDate > minDate && x.UserId == userId)
            .OrderBy(x => x.AccountDate)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return data?.AccountDate;
    }
}
