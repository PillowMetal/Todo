using System.Linq;
using static System.String;

namespace Todo.Helpers
{
    public static class StringExtensions
    {
        public static string ToLowerFirstChar(this string value) => Concat(
            value?.Length > 0 ? value.First().ToString().ToLowerInvariant() : Empty,
            value?.Length > 1 ? value[1..] : Empty);
    }
}
