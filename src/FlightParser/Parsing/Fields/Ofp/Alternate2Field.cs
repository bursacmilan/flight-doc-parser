using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;
using FlightParser.Parsing.Fields.Validators;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class Alternate2Field : RegexField<OfpSectionBuilder, string?>
{
    public override string Name => "Alternate2";

    protected override bool IsRequired => false;

    protected override string Label => "ALTN2:";

    protected override Regex Pattern => Altn2Line();

    protected override bool TryConvert(string rawValue, out string? value)
    {
        value = rawValue.Length == 0 ? null : rawValue;
        return value is null || IcaoValidator.IsValid(value);
    }

    protected override void Apply(OfpSectionBuilder builder, string? value) => builder.Alternate2 = value;

    [GeneratedRegex(@"^ALTN2:[ \t]*(?<value>\S+(?=\s+[A-Z]{3}\b))?", RegexOptions.Multiline)]
    private static partial Regex Altn2Line();
}
