namespace FlightParser.Pdf;

internal interface IPdfTextExtractor
{
    Task<PdfReadResult> ExtractAsync(Stream pdf, CancellationToken cancellationToken);
}
