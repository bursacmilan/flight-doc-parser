using System.Text.RegularExpressions;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;
using FlightParser.Pdf;

namespace FlightParser.Parsing.Fields.Ofp;

internal sealed partial class RouteField : Field<OfpSectionBuilder, (string First, string Last)>
{
    public override string Name => "Route";

    protected override FieldResult<(string First, string Last)> ExtractValue(PageContent page)
    {
        var block = RouteBlock().Match(page.Text);
        if (!block.Success)
        {
            return new FieldResult<(string First, string Last)>.NotFound("Label 'To DEST:' not found.");
        }

        var tokens = block.Groups["value"].Value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return new FieldResult<(string First, string Last)>.NotFound("Value after 'To DEST:' is empty.");
        }

        var rawRoute = string.Join(' ', tokens);
        var first = StripSpeedAndLevel(tokens[0]);
        var last = StripSpeedAndLevel(tokens[^1]);

        return NavPoint().IsMatch(first) && NavPoint().IsMatch(last)
            ? new FieldResult<(string First, string Last)>.Found((first, last))
            : new FieldResult<(string First, string Last)>.Invalid(rawRoute, $"Value '{rawRoute}' after 'To DEST:' is not in the expected format.");
    }

    protected override void Apply(OfpSectionBuilder builder, (string First, string Last) value)
    {
        builder.RouteFirstNavPoint = value.First;
        builder.RouteLastNavPoint = value.Last;
    }

    private static string StripSpeedAndLevel(string token) =>
        token.Split('/')[0];

    [GeneratedRegex(@"^To DEST:[ \t]*(?<value>.*?)(?=^To ALTN1:|\z)", RegexOptions.Multiline | RegexOptions.Singleline)]
    private static partial Regex RouteBlock();

    [GeneratedRegex(@"^[A-Z0-9]{2,11}$")]
    private static partial Regex NavPoint();
}
