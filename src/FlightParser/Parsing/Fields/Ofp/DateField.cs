using System.Globalization;
using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class DateField : RegexField<OfpSectionBuilder, DateOnly>
{
    public override string Name => "Date";

    protected override string Label => "Date:";

    protected override Regex Pattern => DateLine();

    protected override bool TryConvert(string rawValue, out DateOnly value) =>
        DateOnly.TryParseExact(rawValue, "ddMMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out value);

    protected override void Apply(OfpSectionBuilder builder, DateOnly value) => builder.Date = value;

    [GeneratedRegex(@"^Date:\s+(?<value>\S+)", RegexOptions.Multiline)]
    private static partial Regex DateLine();
}
