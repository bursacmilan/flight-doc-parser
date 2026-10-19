using System.Text.RegularExpressions;
using FlightParser.Pdf;

namespace FlightParser.Parsing.PageClassifiers;

internal sealed partial class CrewBriefingPageClassifier : IPageClassifier
{
    private const string Title = "Flight Assignment / Flight Crew Briefing";

    public bool IsMatch(PageContent page) =>
        page.Text.Contains(Title, StringComparison.Ordinal) && FirstPageFooter().IsMatch(page.Text);

    [GeneratedRegex(@"^Page 1 of \d+\b", RegexOptions.Multiline)]
    private static partial Regex FirstPageFooter();
}
