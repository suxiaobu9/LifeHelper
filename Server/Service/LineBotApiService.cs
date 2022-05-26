using isRock.LineBot;
using LifeHelper.Server.Models.Flex;
using LifeHelper.Server.Models.LineApi;
using LifeHelper.Server.Models.Template;
using LifeHelper.Shared.Enum;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace LifeHelper.Server.Service;

public class LineBotApiService
{
    private readonly AccountingRepository accountingRepository;
    private readonly DeleteAccountRepository deleteAccountRepository;
    private readonly UnitOfWork unitOfWork;
    private readonly string IntRegex = @"-?\d+";

    public LineBotApiService(AccountingRepository accountingRepository,
        DeleteAccountRepository deleteAccountRepository,
        UnitOfWork unitOfWork)
    {
        this.accountingRepository = accountingRepository;
        this.deleteAccountRepository = deleteAccountRepository;
        this.unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 處理訊息
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<LineReplyModel> AnalyzeMessages(Event lineEvent, User user)
    {
        // 是否含整數
        if (!string.IsNullOrWhiteSpace(lineEvent.message.text) &&
            Regex.IsMatch(lineEvent.message.text, IntRegex))
            return await Accounting(lineEvent, user);

        return new LineReplyModel(LineReplyEnum.Message, "好的");
    }

    /// <summary>
    /// Line 的 postback 動作
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <returns></returns>
    public async Task<LineReplyModel> Postback(Event lineEvent)
    {
        // 取得帳務 Id
        var convertSuccess = int.TryParse(lineEvent.postback.data, out int accountId);

        if (!convertSuccess)
            return new LineReplyModel(LineReplyEnum.Message, "轉換失敗");

        var account = await accountingRepository.GetAccounting(accountId, lineEvent.source.userId);

        if (account == null)
            return new LineReplyModel(LineReplyEnum.Message, "查無帳務");

        // 取得刪除帳務的資訊
        var deleteEvent = await deleteAccountRepository.GetDeleteAccount(accountId);

        var result = new ConfirmModel
        {
            AccountId = accountId,
            EventName = account.Event,
            Pay = account.Amount
        };

        var utcNow = DateTime.UtcNow;

        // 沒有刪除帳務的資訊
        if (deleteEvent == null)
        {
            await deleteAccountRepository.AddAsync(new DeleteAccount
            {
                AccountId = accountId,
                Deadline = utcNow.AddMinutes(5),
            });
            await unitOfWork.CompleteAsync();

            return new LineReplyModel(LineReplyEnum.Json, FlexTemplate.DeleteAccountingComfirm(result));
        }

        // 刪除帳務的資訊過期
        if (deleteEvent.Deadline < utcNow)
        {
            deleteEvent.Deadline = utcNow.AddMinutes(5);
            await unitOfWork.CompleteAsync();
            return new LineReplyModel(LineReplyEnum.Json, FlexTemplate.DeleteAccountingComfirm(result));
        }

        deleteAccountRepository.Remove(deleteEvent);
        accountingRepository.Remove(account);

        await unitOfWork.CompleteAsync();
        return new LineReplyModel(LineReplyEnum.Message, "刪除成功");
    }

    /// <summary>
    /// 記帳
    /// </summary>
    /// <param name="lineEvent"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<LineReplyModel> Accounting(Event lineEvent, User user)
    {
        var sourceMsg = lineEvent.message.text.Replace(Environment.NewLine, "");

        // 訊息中是否有整數
        var intRegex = Regex.Match(sourceMsg, IntRegex);

        if (!intRegex.Success)
            return new LineReplyModel(LineReplyEnum.Message, "取值錯誤");

        if (!int.TryParse(intRegex.Value, out int amount))
            return new LineReplyModel(LineReplyEnum.Message, "型別轉換錯誤");

        var eventName = sourceMsg.Replace(amount.ToString(), "").Replace("\n","");

        if (string.IsNullOrWhiteSpace(eventName))
            eventName = "其他";

        // 記帳
        var utcNow = DateTime.UtcNow;

        var accounting = new Accounting
        {
            AccountDate = utcNow,
            CreateDate = utcNow,
            Amount = amount,
            UserId = user.Id,
            Event = eventName,
        };

        await accountingRepository.AddAsync(accounting);
        await unitOfWork.CompleteAsync();

        // 取得月帳務
        var monthlyAccountings = await accountingRepository.GetMonthlyAccounting(user.Id);

        var flexMessageModel = new AccountingFlexMessageModel
        {
            AccountId = accounting.Id,
            MonthlyOutlay = monthlyAccountings.Where(x => x.Amount > 0).Sum(x => x.Amount),
            MonthlyIncome = Math.Abs(monthlyAccountings.Where(x => x.Amount < 0).Sum(x => x.Amount)),
            EventName = accounting.Event,
            Pay = amount,
            CreateDate = utcNow.AddHours(8)
        };

        return new LineReplyModel(LineReplyEnum.Json, FlexTemplate.AccountingFlexMessageTemplate(flexMessageModel));
    }
}
