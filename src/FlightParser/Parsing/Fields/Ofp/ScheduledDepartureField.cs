using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class ScheduledDepartureField : RegexField<OfpSectionBuilder, TimeOnly>
{
    public override string Name => "ScheduledDeparture";

    protected override string Label => "STD:";

    protected override Regex Pattern => Std();

    protected override bool TryConvert(string rawValue, out TimeOnly value) => FieldValues.TryParseTimeOfDay(rawValue, out value);

    protected override void Apply(OfpSectionBuilder builder, TimeOnly value) => builder.ScheduledDepartureUtc = value;

    [GeneratedRegex(@"\bSTD:\s+(?<value>\S+)")]
    private static partial Regex Std();
}
