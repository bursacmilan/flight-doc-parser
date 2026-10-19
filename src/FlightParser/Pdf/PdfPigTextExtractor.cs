using FlightParser.Issues;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Exceptions;
using UglyToad.PdfPig.Graphics;

namespace FlightParser.Pdf;

internal sealed class PdfPigTextExtractor : IPdfTextExtractor
{
    private const double LineThickness = 1.5;

    public async Task<PdfReadResult> ExtractAsync(Stream pdf, CancellationToken cancellationToken)
    {
        if (pdf.CanSeek)
        {
            return Extract(pdf, cancellationToken);
        }

        // PdfPig requires a seekable stream, so we copy the content to a MemoryStream if the input stream is not seekable.
        await using var buffer = new MemoryStream();
        await pdf.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        buffer.Seek(0, SeekOrigin.Begin);

        return Extract(buffer, cancellationToken);
    }

    private static PdfReadResult Extract(Stream source, CancellationToken cancellationToken)
    {
        try
        {
            using var document = PdfDocument.Open(source);

            var pages = new List<PageContent>(document.NumberOfPages);
            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                pages.Add(ToPageContent(page));
            }

            return new PdfReadResult.Success(pages);
        }
        catch (PdfDocumentEncryptedException)
        {
            return Failure(IssueCodes.PdfPasswordProtected, "The PDF is password protected.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Failure(IssueCodes.PdfUnreadable, $"The PDF could not be read: {exception.Message}");
        }
    }

    private static PageContent ToPageContent(Page page)
    {
        var words = page.GetWords()
            .Where(word => !string.IsNullOrWhiteSpace(word.Text))
            .Select(word => new PageWord(
                word.Text,
                word.BoundingBox.Top, word.BoundingBox.Right, word.BoundingBox.Bottom, word.BoundingBox.Left))
            .ToList();

        var text = words.ToText();
        return new PageContent(page.Number, text, words) { Lines = ToLineSegments(page) };
    }

    private static List<LineSegment> ToLineSegments(Page page)
    {
        var segments = new List<LineSegment>();

        foreach (var path in page.Paths.Where(path => !path.IsClipping))
        {
            foreach (var subpath in path)
            {
                if (path.IsFilled && subpath.GetBoundingRectangle() is { } box && (box.Width <= LineThickness || box.Height <= LineThickness))
                {
                    AddSegment(segments, box.Left, box.Centroid.Y, box.Right, box.Centroid.Y);
                    AddSegment(segments, box.Centroid.X, box.Bottom, box.Centroid.X, box.Top);
                }
                else if (path.IsStroked)
                {
                    AddStrokedEdges(segments, subpath);
                }
            }
        }

        return segments;
    }

    private static void AddStrokedEdges(List<LineSegment> segments, PdfSubpath subpath)
    {
        PdfPoint? start = null;
        PdfPoint? current = null;

        foreach (var command in subpath.Commands)
        {
            switch (command)
            {
                case PdfSubpath.Move move:
                    start = current = move.Location;
                    break;
                case PdfSubpath.Line line:
                    AddSegment(segments, line.From.X, line.From.Y, line.To.X, line.To.Y);
                    current = line.To;
                    break;
                case PdfSubpath.Close when start is { } first && current is { } last:
                    AddSegment(segments, last.X, last.Y, first.X, first.Y);
                    current = start;
                    break;
            }
        }
    }

    private static void AddSegment(List<LineSegment> segments, double x1, double y1, double x2, double y2)
    {
        var width = Math.Abs(x2 - x1);
        var height = Math.Abs(y2 - y1);

        if (height <= LineThickness && width > LineThickness)
        {
            segments.Add(new LineSegment(LineOrientation.Horizontal, (y1 + y2) / 2, Math.Min(x1, x2), Math.Max(x1, x2)));
        }
        else if (width <= LineThickness && height > LineThickness)
        {
            segments.Add(new LineSegment(LineOrientation.Vertical, (x1 + x2) / 2, Math.Min(y1, y2), Math.Max(y1, y2)));
        }
    }

    private static PdfReadResult.Failure Failure(string code, string message) =>
        new(new ParseIssue(code, message, Chapter: null, Field: null, PageNumber: null, RawText: null));
}
