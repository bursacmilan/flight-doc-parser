using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class DestinationAirportField : AirportField<OfpSectionBuilder>
{
    public override string Name => "DestinationAirport";

    protected override string Label => "To:";

    protected override Regex Pattern => ToLine();

    protected override void Apply(OfpSectionBuilder builder, string value) => builder.DestinationAirport = value;

    [GeneratedRegex(@"^To:\s+(?<value>\S+)", RegexOptions.Multiline)]
    private static partial Regex ToLine();
}
