using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;

namespace FlightParser.Parsing.Fields.Crew;

internal sealed partial class DryOperatingWeightField : RegexField<CrewBriefingSectionBuilder, int>
{
    private const string Kilograms = "kg";

    public override string Name => "DryOperatingWeight";

    protected override string Label => "DOW:";

    protected override Regex Pattern => DowLine();

    protected override bool TryConvert(string rawValue, out int value) =>
        FieldValues.TryParseInteger(rawValue.EndsWith(Kilograms, StringComparison.Ordinal) ? rawValue[..^Kilograms.Length] : rawValue, out value);

    protected override void Apply(CrewBriefingSectionBuilder builder, int value) => builder.DryOperatingWeightKg = value;

    [GeneratedRegex(@"\bDOW:\s+(?<value>\S+)")]
    private static partial Regex DowLine();
}
