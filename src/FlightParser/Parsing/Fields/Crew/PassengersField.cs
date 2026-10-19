using System.Globalization;
using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;
using FlightParser.Parsing.Fields.Values;

namespace FlightParser.Parsing.Fields.Crew;

internal sealed partial class PassengersField : RegexField<CrewBriefingSectionBuilder, PassengerCount>
{
    public override string Name => "Passengers";

    protected override string Label => "PAX C/Y";

    protected override Regex Pattern => FlightRow();

    protected override bool TryConvert(string rawValue, out PassengerCount value)
    {
        var match = BusinessAndEconomy().Match(rawValue);
        value = match.Success
            ? new PassengerCount(
                int.Parse(match.Groups["business"].Value, CultureInfo.InvariantCulture),
                int.Parse(match.Groups["economy"].Value, CultureInfo.InvariantCulture))
            : new PassengerCount(0, 0);
        
        return match.Success;
    }

    protected override void Apply(CrewBriefingSectionBuilder builder, PassengerCount value)
    {
        builder.PassengersBusiness = value.Business;
        builder.PassengersEconomy = value.Economy;
    }

    [GeneratedRegex(@"^(?:[A-Z]{3}\s+){2}\d{2}:\d{2}\s+\d{2}:\d{2}\s+\S+\s+(?<value>\S+)", RegexOptions.Multiline)]
    private static partial Regex FlightRow();

    [GeneratedRegex(@"^(?<business>\d{1,3})/(?<economy>\d{1,3})$")]
    private static partial Regex BusinessAndEconomy();
}
