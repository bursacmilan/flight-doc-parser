using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class MinimumFuelRequiredField : RegexField<OfpSectionBuilder, decimal>
{
    public override string Name => "MinimumFuelRequired";

    protected override string Label => "MIN:";

    protected override Regex Pattern => MinLine();

    protected override bool TryConvert(string rawValue, out decimal value) => FieldValues.TryParseDecimal(rawValue, out value);

    protected override void Apply(OfpSectionBuilder builder, decimal value) => builder.MinimumFuelRequiredTons = value;

    [GeneratedRegex(@"^MIN:\s+\S+\s+(?<value>\S+)", RegexOptions.Multiline)]
    private static partial Regex MinLine();
}
