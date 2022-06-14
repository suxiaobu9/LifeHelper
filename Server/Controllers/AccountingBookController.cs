using LifeHelper.Server.Attributes.Authorize;
using LifeHelper.Shared.Models.LIFF;
using Microsoft.AspNetCore.Mvc;

namespace LifeHelper.Server.Controllers;

[Route("[controller]")]
[LineIdTokenAuthorize]
public class AccountingBookController : Controller
{
    private readonly AccountingService accountingService;
    private readonly UserProfile? userProfile;
    public AccountingBookController(AccountingService accountingService,
        UserProfileService userProfileService)
    {
        this.accountingService = accountingService;
        this.userProfile = userProfileService.UserProfile;
    }

    [HttpGet("MonthlyAccounting/{dateTicks:long}")]
    public async Task<MonthlyAccountingVm?> MonthlyAccountingAsync(long dateTicks)
    {
        if (userProfile == null)
            return null;

        var utcDate = new DateTime(dateTicks);

        return await accountingService.MonthlyAccountingAsync(userProfile, utcDate);
    }
}
