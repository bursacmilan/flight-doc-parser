using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;
using FlightParser.Parsing.Fields.Validators;

namespace FlightParser.Parsing.Fields.Crew;

internal sealed partial class CrewFlightNumberField : RegexField<CrewBriefingSectionBuilder, string>
{
    public override string Name => "FlightNumber";

    protected override string Label => "flight number below the flight date";

    protected override Regex Pattern => LineBelowDate();

    protected override bool TryConvert(string rawValue, out string value)
    {
        value = rawValue;
        return FlightNumberValidator.IsValid(rawValue);
    }

    protected override void Apply(CrewBriefingSectionBuilder builder, string value) => builder.FlightNumber = value;

    [GeneratedRegex(@"^\d{1,2}\.[A-Za-z]{3}\.\d{4}\b[^\n]*\n(?<value>\S+)", RegexOptions.Multiline)]
    private static partial Regex LineBelowDate();
}
