using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class Alternate1Field : AirportField<OfpSectionBuilder>
{
    public override string Name => "Alternate1";

    protected override string Label => "ALTN1:";

    protected override Regex Pattern => Altn1Line();

    protected override void Apply(OfpSectionBuilder builder, string value) => builder.Alternate1 = value;

    [GeneratedRegex(@"^ALTN1:\s+(?<value>\S+)", RegexOptions.Multiline)]
    private static partial Regex Altn1Line();
}
