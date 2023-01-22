using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeHelper.Shared.Const;

public static class BlobConst
{
    public static string AccountingBlobName(Guid userId, Guid accountingId) => $"accounting/{userId}/{DateTime.UtcNow:yyyyMM}/{accountingId}.json";

    public static string MonthlyAccountingDirectory(Guid userId, DateTime? utcNow = null) => $"accounting/{userId}/{utcNow ?? DateTime.UtcNow:yyyyMM}/";

    public static string UserAccountingDirectory(Guid userId) => $"accounting/{userId}/";

    public static string UserBlobName(string userLineId) => $"user/{userLineId}.json";

    public static string UserBlobDirectory => "user/";

    public static string DeleteConfirmBlobName(Guid deleteConfirmId, Guid userId, string featureName) => $"deleteconfirm/{userId}/{featureName}/{deleteConfirmId}.json";

    public static string MemorandumBlobName(Guid userId, Guid memorandumId) => $"memorandum/{userId}/{memorandumId}.json";

    public static string MemorandumBlobs(Guid userId) => $"memorandum/{userId}/";
}
