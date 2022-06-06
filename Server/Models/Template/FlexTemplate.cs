using LifeHelper.Server.Models.Flex;

namespace LifeHelper.Server.Models.Template;

public class FlexTemplate
{
    private static string FlexMessage(string message) => @"
[{
    ""type"": ""flex"",
    ""altText"": ""新訊息"",
    ""contents"":
        " + message + @"
}]";

    /// <summary>
    /// 記帳明細
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<string> AccountingFlexMessageWithLastestEventTemplate(AccountingFlexMessageModel model)
    {
        using var stream = new StreamReader("json\\AccountingFlexTemplate.json");
        var template = await stream.ReadToEndAsync();

        template = template
                    .Replace("{0}", model.Pay >= 0 ? "支出" : "收入")
                    .Replace("{1}", model.MonthlyOutlay.ToString())
                    .Replace("{2}", model.MonthlyIncome.ToString())
                    .Replace("{3}", model.EventName?.ToUnicodeString())
                    .Replace("{4}", Math.Abs(model.Pay).ToString())
                    .Replace("{5}", JsonSerializer.Serialize(model.DeleteConfirm).Base64Encode())
                    .Replace("{6}", model.CreateDate.ToString("yyyy-MM-dd HH:mm"));

        return FlexMessage(template);
    }

    /// <summary>
    /// 記帳明細
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<string> AccountingFlexMessageTemplate(AccountingFlexMessageModel model)
    {
        using var stream = new StreamReader("json\\AccountingFlexTemplate.json");
        var template = await stream.ReadToEndAsync();

        var flexModel = JsonSerializer.Deserialize<BubbleContainer>(template);

        var flexContent = flexModel.Body.Contents[..4].ToList();

        flexContent.AddRange(flexModel.Body.Contents[8..].ToList());

        flexModel.Body.Contents = flexContent.ToArray();

        template = JsonSerializer.Serialize(flexModel);

        template = template
               .Replace("{0}", "總和")
               .Replace("{1}", model.MonthlyOutlay.ToString())
               .Replace("{2}", model.MonthlyIncome.ToString())
               .Replace("{6}", model.CreateDate.ToString("yyyy-MM-dd HH:mm"));

        return FlexMessage(template);
    }

    /// <summary>
    /// 刪除資料的確認畫面
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<string> DeleteComfirmFlexTemplate(string description, FlexDeleteConfirmModel model)
    {
        using var stream = new StreamReader("json\\DeleteComfirmFlexTemplate.json");
        var template = await stream.ReadToEndAsync();

        template = template.Replace("{0}", model.FeatureDisplay)
                            .Replace("{1}", description.ToUnicodeString())
                            .Replace("{2}", JsonSerializer.Serialize(model).Base64Encode());

        return FlexMessage(template);
    }

    /// <summary>
    /// 備忘錄
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<string> MemorandumFlexMessageTemplate(Memorandum[] memoes)
    {
        using var templateStream = new StreamReader("json\\MemorandumFlexMessageTemplate.json");
        var template = await templateStream.ReadToEndAsync();
        var flexModel = JsonSerializer.Deserialize<BubbleContainer>(template);

#pragma warning disable CS8602 // 可能 null 參考的取值 (dereference)。
        var bodyTemplate = flexModel.Body.Contents[1];
#pragma warning restore CS8602 // 可能 null 參考的取值 (dereference)。

        var memoBody = new List<BlockContent> { flexModel.Body.Contents[0] };
        foreach (var memo in memoes)
        {
            var flexDeleteConfirmModel = new FlexDeleteConfirmModel(null, nameof(Memorandum), memo.Id);
            var postbackJson = JsonSerializer.Serialize(flexDeleteConfirmModel);

            var memoItemJson = JsonSerializer.Serialize(bodyTemplate);

            memoItemJson = memoItemJson
                 .Replace("{0}", memo.Memo.ToUnicodeString())
                 .Replace("{1}", postbackJson.Base64Encode());

#pragma warning disable CS8604 // 可能有 Null 參考引數。
            memoBody.Add(JsonSerializer.Deserialize<BlockContent>(memoItemJson));
#pragma warning restore CS8604 // 可能有 Null 參考引數。
        }

        flexModel.Body.Contents = memoBody.ToArray();

        template = JsonSerializer.Serialize(flexModel);

        return FlexMessage(template);
    }
}
