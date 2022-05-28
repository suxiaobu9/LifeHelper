using LifeHelper.Shared.Models.LIFF;
using Microsoft.EntityFrameworkCore;

namespace LifeHelper.Server.Service;

public class AccountingService
{
    private readonly UserProfile? userProfile;
    private readonly AccountingRepository accountingRepository;
    private readonly UserService userService;

    public AccountingService(UserProfileService userProfileService,
        UserService userService,
        AccountingRepository accountingRepository)
    {
        this.userProfile = userProfileService.UserProfile;
        this.accountingRepository = accountingRepository;
        this.userService = userService;
    }

    /// <summary>
    /// 取得月份帳務資料
    /// </summary>
    /// <returns></returns>
    public async Task<MonthlyAccountingVm?> MonthlyAccounting()
    {
        if (userProfile == null)
            return null;

        var user = await userService.UpsertCurrentUser();

        if (user == null)
            return null;

        var twNow = DateTime.UtcNow.AddHours(8);

        var tmp = await accountingRepository.GetMonthlyAccounting(user.Id, twNow);

        var result = new MonthlyAccountingVm
        {
            Month = twNow.Month,
            Income = tmp.Where(x => x.Amount < 0)
                        .Select(x => new MonthlyAccountingVm.EventDetail(x.AccountDate, x.Event, x.Amount))
                        .ToArray(),
            Outlay = tmp.Where(x => x.Amount > 0)
                        .OrderByDescending(x=>x.AccountDate)
                        .Select(x => new MonthlyAccountingVm.EventDetail(x.AccountDate, x.Event, x.Amount))
                        .ToArray(),
        };

        return result;

    }

}
