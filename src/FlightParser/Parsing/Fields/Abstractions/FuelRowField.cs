using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Values;
using FlightParser.Pdf;

namespace FlightParser.Parsing.Fields.Abstractions;

internal abstract class FuelRowField<TBuilder> : Field<TBuilder, FuelRow>
{
    protected abstract Regex AirportLine { get; }

    protected abstract string AirportLabel { get; }

    protected sealed override FieldResult<FuelRow> ExtractValue(PageContent page)
    {
        var airport = AirportLine.Match(page.Text);
        if (!airport.Success)
        {
            return new FieldResult<FuelRow>.NotFound($"Label '{AirportLabel}' not found.");
        }

        var code = airport.Groups["value"].Value;
        var row = Regex.Match(page.Text, $@"^{Regex.Escape(code)}:\s+(?<time>\S+)\s+(?<fuel>\S+)", RegexOptions.Multiline | RegexOptions.CultureInvariant);
        if (!row.Success)
        {
            return new FieldResult<FuelRow>.NotFound($"Label '{code}:' not found.");
        }

        var rawTime = row.Groups["time"].Value;
        var rawFuel = row.Groups["fuel"].Value;

        return FieldValues.TryParseDuration(rawTime, out var time) && FieldValues.TryParseDecimal(rawFuel, out var fuel)
            ? new FieldResult<FuelRow>.Found(new FuelRow(time, fuel))
            : new FieldResult<FuelRow>.Invalid(row.Value, $"Value '{rawTime} {rawFuel}' after '{code}:' is not in the expected format.");
    }
}
