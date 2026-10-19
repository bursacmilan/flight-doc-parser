namespace FlightParser.Pdf;

internal sealed record PageContent(int PageNumber, string Text, IReadOnlyList<PageWord> Words)
{
    public IReadOnlyList<LineSegment> Lines { get; init; } = [];
}
