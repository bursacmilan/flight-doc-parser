using FlightParser.Issues;

namespace FlightParser.Pdf;

internal abstract record PdfReadResult
{
    private PdfReadResult()
    {
    }

    public sealed record Success(IReadOnlyList<PageContent> Pages) : PdfReadResult;

    public sealed record Failure(ParseIssue Issue) : PdfReadResult;
}
