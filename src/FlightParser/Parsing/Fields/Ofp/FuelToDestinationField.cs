using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;
using FlightParser.Parsing.Fields.Values;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class FuelToDestinationField : FuelRowField<OfpSectionBuilder>
{
    public override string Name => "TimeAndFuelToDestination";

    protected override string AirportLabel => "To:";

    protected override Regex AirportLine => ToLine();

    protected override void Apply(OfpSectionBuilder builder, FuelRow value)
    {
        builder.TimeToDestination = value.Time;
        builder.FuelToDestinationTons = value.FuelTons;
    }

    [GeneratedRegex(@"^To:\s+(?<value>[A-Z]{4})\b", RegexOptions.Multiline)]
    private static partial Regex ToLine();
}
