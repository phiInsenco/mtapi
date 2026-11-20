using System;
using System.Globalization;

namespace MtApi5
{
    /// <summary>
    /// Helpers to read MT5 times returned by the EA.
    /// All values are broker SERVER time; results keep Kind = Unspecified (no timezone conversion).
    /// Consumers convert to UTC/local using the broker's offset.
    /// </summary>
    internal static class Mt5ServerTime
    {
        private static readonly string[] Formats =
        {
            "yyyy.MM.dd HH:mm:ss", "yyyy.MM.dd HH:mm", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm"
        };

        /// <summary>
        /// MT5 *_MSC values are milliseconds since 1970-01-01 counted in SERVER time (not real UTC Unix time).
        /// Returns null when msc &lt;= 0.
        /// </summary>
        public static DateTime? FromMsc(long msc) =>
            msc > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(msc).DateTime : null; // .DateTime => Unspecified

        /// <summary>Parses the TimeToString() output ("yyyy.MM.dd HH:mm:ss").</summary>
        public static DateTime? Parse(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParseExact(s, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;
            return null;
        }

        /// <summary>Prefers the millisecond value (higher precision); falls back to the string when msc is 0.</summary>
        public static DateTime? FromMscOrString(long msc, string? s) => FromMsc(msc) ?? Parse(s);
    }
}
