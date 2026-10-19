using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;
using FlightParser.Parsing.Fields.Values;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class FuelToAlternateField : FuelRowField<OfpSectionBuilder>
{
    public override string Name => "TimeAndFuelToAlternate";

    protected override string AirportLabel => "ALTN1:";

    protected override Regex AirportLine => Altn1Line();

    protected override void Apply(OfpSectionBuilder builder, FuelRow value)
    {
        builder.TimeToAlternate = value.Time;
        builder.FuelToAlternateTons = value.FuelTons;
    }

    [GeneratedRegex(@"^ALTN1:\s+(?<value>[A-Z]{4})\b", RegexOptions.Multiline)]
    private static partial Regex Altn1Line();
}
