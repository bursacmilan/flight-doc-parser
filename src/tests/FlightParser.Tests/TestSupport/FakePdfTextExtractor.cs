using FlightParser.Pdf;

namespace FlightParser.Tests.TestSupport;

internal sealed class FakePdfTextExtractor(PdfReadResult result) : IPdfTextExtractor
{
    public Task<PdfReadResult> ExtractAsync(Stream pdf, CancellationToken cancellationToken) => Task.FromResult(result);
}
