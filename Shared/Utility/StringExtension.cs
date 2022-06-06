using System.Text;

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
    }
}
