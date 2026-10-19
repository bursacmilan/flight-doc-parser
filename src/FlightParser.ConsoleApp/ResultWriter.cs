using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FlightParser.Issues;
using FlightParser.Results;

namespace FlightParser.ConsoleApp;

internal static class ResultWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
    };

    public static int Write(FlightPlanParseResult result, TextWriter output, TextWriter errors) =>
        result.Match(
            parsed => WriteParsed(parsed, output, errors),
            rejected => WriteRejected(rejected, errors));

    private static int WriteParsed(FlightPlanParseResult.Parsed parsed, TextWriter output, TextWriter errors)
    {
        output.WriteLine(JsonSerializer.Serialize(parsed.ValidFlights, JsonOptions));

        foreach (var flight in parsed.FailedFlights)
        {
            errors.WriteLine($"Flight {flight.FlightNumber ?? "(unknown)"} on {FormatDate(flight.Date)} was discarded:");
            foreach (var issue in flight.Issues)
            {
                WriteIssue(errors, flight.FlightNumber, issue);
            }
        }

        errors.WriteLine($"{parsed.ValidFlights.Count} valid and {parsed.FailedFlights.Count} discarded flights.");

        return parsed.HasFailedFlights ? ExitCodes.SomeFlightsFailed : ExitCodes.AllFlightsValid;
    }

    private static int WriteRejected(FlightPlanParseResult.Rejected rejected, TextWriter errors)
    {
        errors.WriteLine("The document was rejected:");
        foreach (var issue in rejected.Issues)
        {
            WriteIssue(errors, flightNumber: null, issue);
        }

        return ExitCodes.DocumentRejected;
    }

    private static void WriteIssue(TextWriter errors, string? flightNumber, ParseIssue issue)
    {
        var flight = flightNumber is null ? string.Empty : $" {flightNumber}";
        var page = issue.PageNumber is { } number ? $" (page {number})" : string.Empty;
        var rawText = issue.RawText is null ? string.Empty : $" [text: '{issue.RawText}']";

        errors.WriteLine($"  {issue.Code}{flight}: {issue.Message}{page}{rawText}");
    }

    private static string FormatDate(DateOnly? date) =>
        date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "(unknown date)";
}
