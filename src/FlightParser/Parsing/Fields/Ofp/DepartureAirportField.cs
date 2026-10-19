using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class DepartureAirportField : AirportField<OfpSectionBuilder>
{
    public override string Name => "DepartureAirport";

    protected override string Label => "From:";

    protected override Regex Pattern => FromLine();

    protected override void Apply(OfpSectionBuilder builder, string value) => builder.DepartureAirport = value;

    [GeneratedRegex(@"^From:\s+(?<value>\S+)", RegexOptions.Multiline)]
    private static partial Regex FromLine();
}
