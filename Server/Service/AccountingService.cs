using LifeHelper.Server.Models.Flex;
using LifeHelper.Server.Models.LineApi;
using LifeHelper.Server.Models.Template;
using LifeHelper.Shared.Const;
using LifeHelper.Shared.Enum;
using LifeHelper.Shared.Models.LIFF;
using static LifeHelper.Shared.Models.LIFF.MonthlyAccountingVm;

namespace LifeHelper.Server.Service;

public class AccountingService : IAccountingService
{
    private readonly AzureBlobStorageService azureBlobStorageService;
    private readonly IUserService userService;
    public AccountingService(AzureBlobStorageService azureBlobStorageService,
        IUserService userService)
    {
        this.azureBlobStorageService = azureBlobStorageService;
        this.userService = userService;
    }


    /// <summary>
    /// 記帳
    /// </summary>
    /// <param name="sourceMsg"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<LineReplyModel> AccountingAsync(string sourceMsg, Guid userId)
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
        var monthlyAccountings = (await azureBlobStorageService.GetBlobs<Accounting>(BlobConst.MonthlyAccountingDirectory(userId))).ToArray();

        var flexMessageModel = new AccountingFlexMessageModel
        {
            MonthlyOutlay = monthlyAccountings.Where(x => x.Amount > 0).Sum(x => x.Amount),
            MonthlyIncome = Math.Abs(monthlyAccountings.Where(x => x.Amount < 0).Sum(x => x.Amount)),
            EventName = eventName,
            Pay = amount,
            CreateDate = DateTime.UtcNow.AddHours(8),
            DeleteConfirm = new FlexDeleteConfirmModel(null, nameof(Accounting), accounting.Id)
        };

        return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.AccountingFlexMessageWithLastestEventTemplateAsync(flexMessageModel));

    }

    /// <summary>
    /// 增加記帳
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="userId"></param>
    /// <param name="eventName"></param>
    /// <returns></returns>
    public async Task<Accounting> AddAccountingAsync(int amount, Guid userId, string eventName)
    {
        var utcNow = DateTime.UtcNow;

        var accounting = new Accounting
        {
            Id = Guid.NewGuid(),
            AccountDate = utcNow,
            CreateDate = utcNow,
            Amount = amount,
            UserId = userId,
            Event = eventName,
        };

        await azureBlobStorageService.UploadBlobAsync(BlobConst.AccountingBlobName(userId, accounting.Id), JsonSerializer.Serialize(accounting));

        return accounting;
    }

    /// <summary>
    /// 取得本月帳務
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<AccountingFlexMessageModel> GetMonthlyAccountingAsync(Guid userId)
    {
        // 取得月帳務
        var monthlyAccountings = (await azureBlobStorageService.GetBlobs<Accounting>(BlobConst.MonthlyAccountingDirectory(userId))).ToArray();

        var flexMessageModel = new AccountingFlexMessageModel
        {
            MonthlyOutlay = monthlyAccountings.Where(x => x.Amount > 0).Sum(x => x.Amount),
            MonthlyIncome = Math.Abs(monthlyAccountings.Where(x => x.Amount < 0).Sum(x => x.Amount)),
            CreateDate = DateTime.UtcNow.AddHours(8),
        };

        return flexMessageModel;
    }

    /// <summary>
    /// 取得月份帳務資料
    /// </summary>
    /// <param name="userProfile"></param>
    /// <param name="utcDate"></param>
    /// <returns></returns>
    public async Task<MonthlyAccountingVm?> MonthlyAccountingAsync(UserProfile userProfile, DateTime? utcDate)
    {
        var user = await userService.UpsertUserAsync(userProfile);

        if (user == null)
            return null;

        utcDate ??= DateTime.UtcNow;

        var monthlyAccountings = (await azureBlobStorageService.GetBlobs<Accounting>(BlobConst.MonthlyAccountingDirectory(user.Id, utcDate))).ToArray() ?? Array.Empty<Accounting>();
        var allAccountingMonth = azureBlobStorageService.GetAllMonth(user.Id);

        DateTime? pre = allAccountingMonth != null && allAccountingMonth.Any(x => x < utcDate) ? allAccountingMonth.Where(x => x < utcDate).Max() : null;
        DateTime? next = allAccountingMonth != null && allAccountingMonth.Any(x => x > utcDate) ? allAccountingMonth.Where(x => x > utcDate).Min() : null;

        var result = new MonthlyAccountingVm
        {
            CurrentAccountingMonth = new AccountingMonth(utcDate.Value.Year, utcDate.Value.Month),
            PreAccountingMonth = pre == null ? null : new AccountingMonth(pre.Value.Year, pre.Value.Month),
            NextAccountingMonth = next == null ? null : new AccountingMonth(next.Value.Year, next.Value.Month),
            Income = monthlyAccountings.Where(x => x.Amount < 0)
                      .Select(x => new EventDetail(x.AccountDate, x.Event, x.Amount))
                      .ToArray(),
            Outlay = monthlyAccountings.Where(x => x.Amount > 0)
                      .OrderByDescending(x => x.AccountDate)
                      .Select(x => new EventDetail(x.AccountDate, x.Event, x.Amount))
                      .ToArray(),
        };

        return result;

    }

    /// <summary>
    /// 刪除記帳
    /// </summary>
    /// <param name="accountingId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task RemoveAccountingAsync(Guid accountingId, Guid userId)
    {
        await azureBlobStorageService.DeleteBlob(BlobConst.AccountingBlobName(userId, accountingId));
    }

}
