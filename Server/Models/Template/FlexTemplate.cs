using LifeHelper.Server.Models.Flex;
using System.Text;

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
    public static async Task<string> AccountingFlexMessageTemplate(AccountingFlexMessageModel model)
    {
        using var stream = new StreamReader("json\\AccountingFlexTemplate.json");
        var template = await stream.ReadToEndAsync();

        template = template
                    .Replace("{0}", model.Pay >= 0 ? "支出" : "收入")
                    .Replace("{1}", model.MonthlyOutlay.ToString())
                    .Replace("{2}", model.MonthlyIncome.ToString())
                    .Replace("{3}", model.EventName)
                    .Replace("{4}", Math.Abs(model.Pay).ToString())
                    .Replace("{5}", Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(model.DeleteConfirm))))
                    .Replace("{6}", model.CreateDate.ToString("yyyy-MM-dd HH:mm"));

        return FlexMessage(template);
    }

    /// <summary>
    /// 刪除資料的確認畫面
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<string> DeleteComfirmFlexTemplate(DeleteConfirmModel model)
    {
        using var stream = new StreamReader("json\\DeleteComfirmFlexTemplate.json");
        var template = await stream.ReadToEndAsync();

        template = template.Replace( "{0}", model.FeatureDisplay)
                            .Replace("{1}", model.Description)
                            .Replace("{2}", Convert.ToBase64String(Encoding.UTF8.GetBytes(model.DeleteConfirmId.ToString())));

        return FlexMessage(template);

    }
}
