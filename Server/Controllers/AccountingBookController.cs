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

    [HttpGet("MonthlyAccounting")]
    public async Task<MonthlyAccountingVm?> MonthlyAccountingAsync()
    {
        if (userProfile == null)
            return null;

        return await accountingService.MonthlyAccountingAsync(userProfile);
    }
}
