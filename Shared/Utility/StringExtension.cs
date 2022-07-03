using LifeHelper.Shared.Const;
using System.Text;
using System.Text.RegularExpressions;

namespace LifeHelper.Shared.Utility
{
    public static class StringExtension
    {
        public static bool JSONTryParse<T>(this string source, out T? result)
        {
            result = default;
            try
            {
                result = JsonSerializer.Deserialize<T>(source);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string Base64Encode(this string source)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(source));
        }

        public static string Base64Decode(this string source)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(source));
        }

        public static string ToUnicodeString(this string source)
        {
            string dst = "";

            foreach (var item in source)
            {
                byte[] bytes = Encoding.Unicode.GetBytes(item.ToString());
                string str = @"\u" + bytes[1].ToString("X2") + bytes[0].ToString("X2");
                dst += str;
            }

            return dst;
        }

        /// <summary>
        /// 訊息是否為記帳
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsAccounting(string source)
        {
            return GetAccountingAmount(source) != null;
        }

        /// <summary>
        /// 取得記帳金額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string? GetAccountingAmount(string source)
        {
            var message = source.Replace(Environment.NewLine, "").Replace("\n", "");

            var regexMatch = Regex.Matches(message, RegexConst.IntRegex);

            foreach (Match item in regexMatch)
            {
                if (item.Success && (message.StartsWith(item.Value) || message.EndsWith(item.Value)))
                    return item.Value;
                continue;
            }

            return null;
        }
    }
}
