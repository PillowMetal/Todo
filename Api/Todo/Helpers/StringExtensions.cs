using System.Linq;
using static System.Globalization.CultureInfo;
using static System.String;

namespace Todo.Helpers
{
    public static class StringExtensions
    {
        public static string ToLowerFirstChar(this string value) =>
            Concat(value?.Length > 0 ? value.First().ToString().ToLower(CurrentCulture) : Empty, value?.Length > 1 ? value.Substring(1) : Empty);
    }
}
