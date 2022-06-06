using LifeHelper.Server.Models.Flex;
using LifeHelper.Server.Models.LineApi;
using LifeHelper.Server.Models.Template;
using LifeHelper.Shared.Const;
using LifeHelper.Shared.Enum;
using LifeHelper.Shared.Models.LIFF;
using System.Text.RegularExpressions;

namespace LifeHelper.Server.Service;

public class AccountingService
{
    private readonly UserProfile? userProfile;
    private readonly AccountingRepository accountingRepository;
    private readonly UserService userService;
    private readonly UnitOfWork unitOfWork;

    public AccountingService(UserProfileService userProfileService,
        UserService userService,
        UnitOfWork unitOfWork,
        AccountingRepository accountingRepository)
    {
        this.userProfile = userProfileService.UserProfile;
        this.accountingRepository = accountingRepository;
        this.userService = userService;
        this.unitOfWork = unitOfWork;
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
                        .OrderByDescending(x => x.AccountDate)
                        .Select(x => new MonthlyAccountingVm.EventDetail(x.AccountDate, x.Event, x.Amount))
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
    public async Task<Accounting> AddAccounting(int amount, int userId, string eventName)
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
    public async Task<LineReplyModel> Accounting(string sourceMsg, int userId)
    {
        sourceMsg = sourceMsg.Replace(Environment.NewLine, "");

        // 訊息中是否有整數
        var intRegex = Regex.Match(sourceMsg, RegexConst.IntRegex);

        if (!intRegex.Success)
            return new LineReplyModel(LineReplyEnum.Message, "取值錯誤");

        if (!int.TryParse(intRegex.Value, out int amount))
            return new LineReplyModel(LineReplyEnum.Message, "型別轉換錯誤");

        var eventName = sourceMsg.Replace(amount.ToString(), "").Replace("\n", "");

        if (string.IsNullOrWhiteSpace(eventName))
            eventName = "其他";

        // 記帳
        var accounting = await AddAccounting(amount, userId, eventName);

        // 取得月帳務
        var monthlyAccountings = await accountingRepository.GetMonthlyAccounting(userId);

        var flexMessageModel = new AccountingFlexMessageModel
        {
            MonthlyOutlay = monthlyAccountings.Where(x => x.Amount > 0).Sum(x => x.Amount),
            MonthlyIncome = Math.Abs(monthlyAccountings.Where(x => x.Amount < 0).Sum(x => x.Amount)),
            EventName = accounting.Event,
            Pay = amount,
            CreateDate = DateTime.UtcNow.AddHours(8),
            DeleteConfirm = new FlexDeleteConfirmModel(null, nameof(Models.EF.Accounting), accounting.Id)
        };

        return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.AccountingFlexMessageWithLastestEventTemplate(flexMessageModel));
    }

    /// <summary>
    /// 刪除記帳
    /// </summary>
    /// <returns></returns>
    public async Task RemoveAccounting(int accountingId, int userId)
    {
        var accounting = await accountingRepository.GetAccounting(accountingId, userId);
        if (accounting == null)
            return;
        accountingRepository.Remove(accounting);

        await unitOfWork.CompleteAsync();
    }

}
