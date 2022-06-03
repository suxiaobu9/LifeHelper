using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

    }
}
