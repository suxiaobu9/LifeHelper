using LifeHelper.Server.Attributes.Authorize;
using LifeHelper.Shared.Models.LIFF;
using Microsoft.AspNetCore.Mvc;

namespace LifeHelper.Server.Controllers;

[Route("[controller]")]
[LineIdTokenAuthorize]
public class AccountingBookController : Controller
{
    private readonly IAccountingService accountingService;
    private readonly UserProfile? userProfile;
    public AccountingBookController(IAccountingService accountingService,
        IUserProfileService userProfileService)
    {
        this.accountingService = accountingService;
        this.userProfile = userProfileService.UserProfile;
    }

    [HttpGet("MonthlyAccounting/{year:int}/{month:int}")]
    public async Task<MonthlyAccountingVm?> MonthlyAccountingAsync(int year, int month)
    {
        if (userProfile == null)
            return null;

        var utcDate = new DateTime(year, month, 1).AddHours(-8);

        return await accountingService.MonthlyAccountingAsync(userProfile, utcDate);
    }
}
