using System.Text.RegularExpressions;

namespace FlightParser.Parsing.Fields.Validators;

internal static partial class IcaoValidator
{
    public static bool IsValid(string code) => IcaoCode().IsMatch(code);

    [GeneratedRegex(@"^[A-Z]{4}$")]
    private static partial Regex IcaoCode();
}
