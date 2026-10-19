using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;
using FlightParser.Parsing.Fields.Validators;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class FlightNumberField : RegexField<OfpSectionBuilder, string>
{
    public override string Name => "FlightNumber";

    protected override string Label => "FltNr:";

    protected override Regex Pattern => FltNrLine();

    protected override bool TryConvert(string rawValue, out string value)
    {
        value = rawValue;
        return FlightNumberValidator.IsValid(rawValue);
    }

    protected override void Apply(OfpSectionBuilder builder, string value) => builder.FlightNumber = value;

    [GeneratedRegex(@"^FltNr:\s+(?<value>\S+)", RegexOptions.Multiline)]
    private static partial Regex FltNrLine();
}
