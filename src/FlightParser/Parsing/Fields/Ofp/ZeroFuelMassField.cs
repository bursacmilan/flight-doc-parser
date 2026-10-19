using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class ZeroFuelMassField : RegexField<OfpSectionBuilder, int>
{
    public override string Name => "ZeroFuelMass";

    protected override string Label => "ZFM:";

    protected override Regex Pattern => Zfm();

    protected override bool TryConvert(string rawValue, out int value) => FieldValues.TryParseInteger(rawValue, out value);

    protected override void Apply(OfpSectionBuilder builder, int value) => builder.ZeroFuelMassKg = value;

    [GeneratedRegex(@"\bZFM:\s+(?<value>\S+)")]
    private static partial Regex Zfm();
}
