using LifeHelper.Server.Models.Flex;
using LifeHelper.Server.Models.LineApi;
using LifeHelper.Server.Models.Template;
using LifeHelper.Shared.Const;
using LifeHelper.Shared.Enum;
using LifeHelper.Shared.Models.LIFF;
using System.Text.RegularExpressions;
using static LifeHelper.Shared.Models.LIFF.MonthlyAccountingVm;

namespace LifeHelper.Server.Service;

public class AccountingService
{
    private readonly AccountingRepository accountingRepository;
    private readonly UserService userService;
    private readonly UnitOfWork unitOfWork;

    public AccountingService(UserService userService,
        UnitOfWork unitOfWork,
        AccountingRepository accountingRepository)
    {
        this.accountingRepository = accountingRepository;
        this.userService = userService;
        this.unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 取得月份帳務資料
    /// </summary>
    /// <returns></returns>
    public async Task<MonthlyAccountingVm?> MonthlyAccountingAsync(UserProfile userProfile, DateTime? utcDate)
    {
        var user = await userService.UpsertUserAsync(userProfile);

        if (user == null)
            return null;

        utcDate ??= DateTime.UtcNow;

        var tmp = await accountingRepository.GetMonthlyAccountingAsync(user.Id, utcDate.Value);

        var pre = (await accountingRepository.GetPreAccouningUtcDateAsync(user.Id, utcDate.Value))?.AddHours(8);

        var next = (await accountingRepository.GetNextAccountingUtcDateAsync(user.Id, utcDate.Value))?.AddHours(8);

        var twDate = utcDate.Value.AddHours(8);

        var result = new MonthlyAccountingVm
        {
            CurrentAccountingMonth = new AccountingMonth(twDate.Year, twDate.Month),
            PreAccountingMonth = pre == null ? null : new AccountingMonth(pre.Value.Year, pre.Value.Month),
            NextAccountingMonth = next == null ? null : new AccountingMonth(next.Value.Year, next.Value.Month),
            Income = tmp.Where(x => x.Amount < 0)
                        .Select(x => new EventDetail(x.AccountDate, x.Event, x.Amount))
                        .ToArray(),
            Outlay = tmp.Where(x => x.Amount > 0)
                        .OrderByDescending(x => x.AccountDate)
                        .Select(x => new EventDetail(x.AccountDate, x.Event, x.Amount))
                        .ToArray(),
        };

        return result;
    }

    /// <summary>
    /// 增加記帳
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="userId"></param>
    /// <param name="eventName"></param>
    /// <returns></returns>
    public async Task<Accounting> AddAccountingAsync(int amount, int userId, string eventName)
    {
        var utcNow = DateTime.UtcNow;

        var accounting = new Accounting
        {
            AccountDate = utcNow,
            CreateDate = utcNow,
            Amount = amount,
            UserId = userId,
            Event = eventName,
        };

        await accountingRepository.AddAsync(accounting);
        await unitOfWork.CompleteAsync();
        return accounting;
    }

    /// <summary>
    /// 記帳
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<LineReplyModel> AccountingAsync(string sourceMsg, int userId)
    {
        sourceMsg = sourceMsg.Replace(Environment.NewLine, "").Replace("\n", "");

        // 訊息中是否有整數
        var amountString = StringExtension.GetAccountingAmount(sourceMsg);

        if (amountString == null)
            return new LineReplyModel(LineReplyEnum.Message, "取值錯誤");

        if (!int.TryParse(amountString, out int amount))
            return new LineReplyModel(LineReplyEnum.Message, "型別轉換錯誤");

        var eventName = sourceMsg.Replace(amount.ToString(), "");

        if (string.IsNullOrWhiteSpace(eventName))
            eventName = "其他";

        // 記帳
        var accounting = await AddAccountingAsync(amount, userId, eventName);

        // 取得月帳務
        var monthlyAccountings = await accountingRepository.GetMonthlyAccountingAsync(userId, DateTime.UtcNow);

        var flexMessageModel = new AccountingFlexMessageModel
        {
            MonthlyOutlay = monthlyAccountings.Where(x => x.Amount > 0).Sum(x => x.Amount),
            MonthlyIncome = Math.Abs(monthlyAccountings.Where(x => x.Amount < 0).Sum(x => x.Amount)),
            EventName = accounting.Event,
            Pay = amount,
            CreateDate = DateTime.UtcNow.AddHours(8),
            DeleteConfirm = new FlexDeleteConfirmModel(null, nameof(Models.EF.Accounting), accounting.Id)
        };

        return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.AccountingFlexMessageWithLastestEventTemplateAsync(flexMessageModel));
    }

    /// <summary>
    /// 刪除記帳
    /// </summary>
    /// <returns></returns>
    public async Task RemoveAccountingAsync(int accountingId, int userId)
    {
        var accounting = await accountingRepository.GetAccountingAsync(accountingId, userId);
        if (accounting == null)
            return;
        accountingRepository.Remove(accounting);

        await unitOfWork.CompleteAsync();
    }

}
