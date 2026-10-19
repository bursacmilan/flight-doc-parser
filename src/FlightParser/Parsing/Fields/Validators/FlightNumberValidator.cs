using System.Text.RegularExpressions;

namespace FlightParser.Parsing.Fields.Validators;

internal static partial class FlightNumberValidator
{
    public static bool IsValid(string flightNumber) => FlightNumber().IsMatch(flightNumber);

    [GeneratedRegex(@"^[A-Z0-9]{2}\d{1,4}[A-Z]?$")]
    private static partial Regex FlightNumber();
}
