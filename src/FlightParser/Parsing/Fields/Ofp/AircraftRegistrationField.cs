using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class AircraftRegistrationField : RegexField<OfpSectionBuilder, string>
{
    public override string Name => "AircraftRegistration";

    protected override string Label => "Reg.:";

    protected override Regex Pattern => RegLine();

    protected override bool TryConvert(string rawValue, out string value)
    {
        value = rawValue;
        return RegistrationFormat().IsMatch(rawValue);
    }

    protected override void Apply(OfpSectionBuilder builder, string value) => builder.AircraftRegistration = value;

    [GeneratedRegex(@"^Reg\.:\s+(?<value>\S+)", RegexOptions.Multiline)]
    private static partial Regex RegLine();

    [GeneratedRegex(@"^[A-Z0-9][A-Z0-9-]{2,7}$")]
    private static partial Regex RegistrationFormat();
}
