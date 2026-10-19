using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class AtcCallSignField : RegexField<OfpSectionBuilder, string>
{
    public override string Name => "AtcCallSign";

    protected override string Label => "ATC:";

    protected override Regex Pattern => AtcLine();

    protected override bool TryConvert(string rawValue, out string value)
    {
        value = rawValue;
        return CallSignFormat().IsMatch(rawValue);
    }

    protected override void Apply(OfpSectionBuilder builder, string value) => builder.AtcCallSign = value;

    [GeneratedRegex(@"^ATC:\s+(?<value>\S+)", RegexOptions.Multiline)]
    private static partial Regex AtcLine();

    [GeneratedRegex(@"^[A-Z][A-Z0-9]{1,6}$")]
    private static partial Regex CallSignFormat();
}
