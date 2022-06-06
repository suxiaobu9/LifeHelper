namespace LifeHelper.Server.Models.Flex;

public class AccountingFlexMessageModel
{
    /// <summary>
    /// 本月花費
    /// </summary>
    public int MonthlyOutlay { get; set; }

    /// <summary>
    /// 本月收入
    /// </summary>
    public int MonthlyIncome { get; set; }

    /// <summary>
    /// 用途
    /// </summary>
    public string? EventName { get; set; }

    /// <summary>
    /// 花費金額
    /// </summary>
    public int Pay { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// 確認刪除的 Id
    /// </summary>
    public FlexDeleteConfirmModel DeleteConfirm { get; set; } = null!;
}
