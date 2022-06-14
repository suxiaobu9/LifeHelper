namespace LifeHelper.Shared.Models.LIFF;

public class MonthlyAccountingVm
{
    /// <summary>
    /// 上一個記帳月
    /// </summary>
    public DateTime? PreAccountingPeriodUtc { get; set; }

    /// <summary>
    /// 記帳月
    /// </summary>
    public DateTime AccountingPeriodUtc { get; set; }

    /// <summary>
    /// 下一個記帳月
    /// </summary>
    public DateTime? NextAccountingPeriodUtc { get; set; }

    /// <summary>
    /// 收入
    /// </summary>
    public EventDetail[]? Income { get; set; }

    /// <summary>
    /// 支出
    /// </summary>
    public EventDetail[]? Outlay { get; set; }

    /// <summary>
    /// 總收入
    /// </summary>
    public int TotalIncome
    {
        get
        {
            if (Income == null)
                return 0;

            return Income.Sum(x => x.Amount);
        }
    }

    /// <summary>
    /// 總支出
    /// </summary>
    public int TotalOutlay
    {
        get
        {
            if (Outlay == null)
                return 0;

            return Outlay.Sum(x => x.Amount);
        }
    }

    /// <summary>
    /// 支出項目統計
    /// </summary>
    public ReportDetail[]? ReportDetails
    {
        get
        {
            if (Income == null || Outlay == null)
                return null;

            var group = Outlay.Select(x => x.Event).Distinct().ToArray();

            return group.Select(x =>
            {
                var total = Outlay.Where(y => y.Event == x).Sum(y => y.Amount);
                return new ReportDetail
                {
                    Event = x,
                    Total = total,
                    Proportion = Math.Round((decimal)total / Outlay.Sum(x => x.Amount) * 100, 2)
                };
            }).OrderByDescending(x => x.Proportion).ToArray();
        }
    }

    /// <summary>
    /// 項目統計
    /// </summary>
    public class ReportDetail
    {
        /// <summary>
        /// 項目
        /// </summary>
        public string? Event { get; set; }

        /// <summary>
        /// 總額
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 百分比
        /// </summary>
        public decimal Proportion { get; set; }
    }

    /// <summary>
    /// 帳務細項
    /// </summary>
    public class EventDetail
    {
        public EventDetail() { }
        public EventDetail(DateTime utcDate, string eventName, int amount)
        {
            Date = utcDate.AddHours(8);
            Event = eventName;
            Amount = Math.Abs(amount);
        }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 項目
        /// </summary>
        public string? Event { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        public int Amount { get; set; }
    }
}
