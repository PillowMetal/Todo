using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static System.String;

namespace Todo.Helpers
{
    public static class StringExtensions
    {
        [SuppressMessage("Design", "CA1308: Replace the call to 'ToLowerInvariant' with 'ToUpperInvariant'", Justification = "<Pending>")]
        public static string ToLowerFirstChar(this string value) => Concat(
            value?.Length > 0 ? value.First().ToString().ToLowerInvariant() : Empty
          , value?.Length > 1 ? value.Substring(1) : Empty);
    }
}
