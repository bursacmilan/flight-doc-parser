using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class ScheduledArrivalField : RegexField<OfpSectionBuilder, TimeOnly>
{
    public override string Name => "ScheduledArrival";

    protected override string Label => "STA:";

    protected override Regex Pattern => Sta();

    protected override bool TryConvert(string rawValue, out TimeOnly value) => FieldValues.TryParseTimeOfDay(rawValue, out value);

    protected override void Apply(OfpSectionBuilder builder, TimeOnly value) => builder.ScheduledArrivalUtc = value;

    [GeneratedRegex(@"\bSTA:\s+(?<value>\S+)")]
    private static partial Regex Sta();
}
