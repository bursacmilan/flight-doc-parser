using System.Text.RegularExpressions;
using FlightParser.Pdf;

namespace FlightParser.Parsing.PageClassifiers;

internal sealed partial class OfpPageClassifier : IPageClassifier
{
    private const string Title = "Operational Flight Plan";

    public bool IsMatch(PageContent page) =>
        page.Text.Contains(Title, StringComparison.Ordinal) && FirstPageHeader().IsMatch(page.Text);

    [GeneratedRegex(@"^FMS IDENT=\S+ Log Nr\.:\s*\d+ Page 1\b", RegexOptions.Multiline)]
    private static partial Regex FirstPageHeader();
}
