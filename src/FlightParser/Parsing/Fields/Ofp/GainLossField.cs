using System.Globalization;
using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class GainLossField : RegexField<OfpSectionBuilder, int>
{
    public override string Name => "GainLoss";

    protected override string Label => "Gain / Loss:";

    protected override Regex Pattern => GainLossLine();

    protected override bool TryConvert(string rawValue, out int value)
    {
        var match = GainLossValue().Match(rawValue);
        if (!match.Success)
        {
            value = 0;
            return false;
        }

        var amount = int.Parse(match.Groups["amount"].Value, CultureInfo.InvariantCulture);
        value = match.Groups["direction"].Value == "LOSS" ? -amount : amount;
        return true;
    }

    protected override void Apply(OfpSectionBuilder builder, int value) => builder.GainLossUsdPerTon = value;

    [GeneratedRegex(@"\bGain / Loss:\s+(?<value>\S+\s+\S+)")]
    private static partial Regex GainLossLine();

    [GeneratedRegex(@"^(?<direction>GAIN|LOSS)\s+(?<amount>\d+)\$/TON$")]
    private static partial Regex GainLossValue();
}
