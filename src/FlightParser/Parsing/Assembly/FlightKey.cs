using System.Globalization;

namespace FlightParser.Parsing.Assembly;

internal readonly record struct FlightKey(string FlightNumber, DateOnly Date)
{
    public static FlightKey? From(string? flightNumber, DateOnly? date) =>
        flightNumber is not null && date is { } value ? new FlightKey(flightNumber, value) : null;

    public override string ToString() => $"{FlightNumber} on {Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
}
