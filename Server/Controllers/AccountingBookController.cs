using LifeHelper.Shared.Models.LIFF;
using Microsoft.AspNetCore.Mvc;

namespace LifeHelper.Server.Controllers;

[Route("[controller]")]
public class AccountingBookController : Controller
{
    private readonly AccountingService accountingService;
    public AccountingBookController(AccountingService accountingService)
    {
        this.accountingService = accountingService;
    }

    [HttpGet("MonthlyAccounting")]
    public async Task<MonthlyAccountingVm?> MonthlyAccounting()
    {
        return await accountingService.MonthlyAccounting();
    }
}
