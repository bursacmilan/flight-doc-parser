using FlightParser.Pdf;
using FlightParser.Tests.TestSupport;

namespace FlightParser.Tests.Pdf;

public sealed class SamplePdfFixture : IAsyncLifetime
{
    internal IReadOnlyList<PageContent> Pages { get; private set; } = [];

    public async ValueTask InitializeAsync()
    {
        await using var pdf = TestData.OpenSamplePdf();
        var result = await new PdfPigTextExtractor().ExtractAsync(pdf, CancellationToken.None);
        Pages = result.ShouldBeOfType<PdfReadResult.Success>().Pages;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    internal PageContent Page(int number) => Pages.Single(page => page.PageNumber == number);
}
