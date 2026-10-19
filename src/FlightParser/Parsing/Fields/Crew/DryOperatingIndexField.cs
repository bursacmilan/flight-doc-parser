using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;

namespace FlightParser.Parsing.Fields.Crew;

internal sealed partial class DryOperatingIndexField : RegexField<CrewBriefingSectionBuilder, decimal>
{
    public override string Name => "DryOperatingIndex";

    protected override string Label => "DOI:";

    protected override Regex Pattern => DoiLine();

    protected override bool TryConvert(string rawValue, out decimal value) => FieldValues.TryParseDecimal(rawValue, out value);

    protected override void Apply(CrewBriefingSectionBuilder builder, decimal value) => builder.DryOperatingIndex = value;

    [GeneratedRegex(@"\bDOI:\s+(?<value>\S+)")]
    private static partial Regex DoiLine();
}
