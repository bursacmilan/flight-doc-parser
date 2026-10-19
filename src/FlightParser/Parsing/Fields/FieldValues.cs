using System.Globalization;
using System.Text.RegularExpressions;

namespace FlightParser.Parsing.Fields;

internal static partial class FieldValues
{
    public static bool TryParseInteger(string raw, out int value) =>
        int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out value);

    public static bool TryParseDecimal(string raw, out decimal value) =>
        decimal.TryParse(raw, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);

    public static bool TryParseTimeOfDay(string raw, out TimeOnly value) =>
        TimeOnly.TryParseExact(raw, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out value);

    public static bool TryParseDuration(string raw, out TimeSpan value)
    {
        var match = Duration().Match(raw);
        value = match.Success
            ? new TimeSpan(int.Parse(match.Groups["hours"].Value, CultureInfo.InvariantCulture), int.Parse(match.Groups["minutes"].Value, CultureInfo.InvariantCulture), 0)
            : default;
        return match.Success;
    }

    [GeneratedRegex(@"^(?<hours>\d{1,2}):(?<minutes>[0-5]\d)$")]
    private static partial Regex Duration();
}
