using System.Globalization;
using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;

namespace FlightParser.Parsing.Fields.Crew;

internal sealed partial class CrewDateField : RegexField<CrewBriefingSectionBuilder, DateOnly>
{
    public override string Name => "Date";

    protected override string Label => "flight date";

    protected override Regex Pattern => DateLine();

    protected override bool TryConvert(string rawValue, out DateOnly value) =>
        DateOnly.TryParseExact(rawValue, "d.MMM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out value);

    protected override void Apply(CrewBriefingSectionBuilder builder, DateOnly value) => builder.Date = value;

    [GeneratedRegex(@"^(?<value>\d{1,2}\.[A-Za-z]{3}\.\d{4})\b", RegexOptions.Multiline)]
    private static partial Regex DateLine();
}
