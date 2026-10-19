using System.Text;
using FlightParser.Issues;
using FlightParser.Pdf;
using FlightParser.Tests.TestSupport;

namespace FlightParser.Tests.Pdf;

public sealed class PdfPigTextExtractorTests(SamplePdfFixture sample) : IClassFixture<SamplePdfFixture>
{
    private readonly PdfPigTextExtractor _extractor = new();

    [Fact]
    public void ExtractAsync_SamplePdf_ReturnsAllPagesInOrder()
    {
        sample.Pages.Select(page => page.PageNumber).ShouldBe(Enumerable.Range(1, 140));
    }

    [Theory]
    [InlineData("Date: 19MAR24 STD: 08:00 STA: 08:55 STE: 0:55")]
    [InlineData("RTE: ZRH-MXP05 ZFM: 34066 kg _______ 40800 kg")]
    [InlineData("LIMC: 0:48 1.7 _____ _____ W ___ 270 0 N/A 0")]
    [InlineData("MIN: 1:43 3.6 _____ _____ 170 10 47.0 1813")]
    [InlineData("Gain / Loss: GAIN 0$/TON")]
    [InlineData("To DEST: VEBIT T53 GERSA/N0401F230 N850 DEGAD Z424 RIXUV")]
    public void ExtractAsync_OfpPage_KeepsLabelsAndValuesOnOneLine(string expectedLine)
    {
        Lines(sample.Page(5)).ShouldContain(expectedLine);
    }

    [Fact]
    public void ExtractAsync_OfpPageWithWrappedRoute_ReturnsRouteOnTwoLines()
    {
        var lines = Lines(sample.Page(26));

        var routeStart = lines.IndexOf("To DEST: ANEKI1Y ANEKI/N0400F230 Y163 NEKLO Y171 INKAM T725 LAMGO");

        routeStart.ShouldBeGreaterThanOrEqualTo(0);
        lines[routeStart + 1].ShouldBe("T721 RILAX");
    }

    [Theory]
    [InlineData("DOW: 28916kg")]
    [InlineData("DOI: 68.7")]
    [InlineData("EZFW: 31536kg")]
    [InlineData("FRA ZRH 13:45 14:40 00:55 1/26")]
    public void ExtractAsync_CrewBriefingPage_KeepsLabelsAndValuesOnOneLine(string expectedText)
    {
        Lines(sample.Page(85)).ShouldContain(line => line.Contains(expectedText, StringComparison.Ordinal));
    }

    [Fact]
    public void ExtractAsync_SamplePdf_ProvidesWordsWithPositions()
    {
        var word = sample.Page(5).Words.First(w => w.Text == "ZFM:");

        word.Right.ShouldBeGreaterThan(word.Left);
        word.Top.ShouldBeGreaterThan(word.Bottom);
    }

    [Fact]
    public void ExtractAsync_FixturePages_MatchStoredFixtures()
    {
        var update = Environment.GetEnvironmentVariable("UPDATE_FIXTURES") == "1";

        foreach (var pageNumber in TestData.FixturePages)
        {
            var actual = sample.Page(pageNumber).Text;

            if (update)
            {
                TestData.WritePageFixture(pageNumber, TestData.WithCrewNamesFrom(actual, TestData.ReadPageFixture(pageNumber)));
            }
            else
            {
                TestData.WithoutCrewNames(actual).ShouldBe(TestData.WithoutCrewNames(TestData.ReadPageFixture(pageNumber)), $"page {pageNumber}");
            }
        }
    }

    [Fact]
    public async Task ExtractAsync_NonSeekableStream_ReadsDocument()
    {
        await using var pdf = new NonSeekableStream(TestData.OpenSamplePdf());

        var result = await _extractor.ExtractAsync(pdf, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<PdfReadResult.Success>().Pages.Count.ShouldBe(140);
    }

    [Fact]
    public async Task ExtractAsync_CallerStream_IsNotDisposed()
    {
        await using var pdf = TestData.OpenSamplePdf();

        await _extractor.ExtractAsync(pdf, TestContext.Current.CancellationToken);

        pdf.CanRead.ShouldBeTrue();
    }

    [Fact]
    public async Task ExtractAsync_InvalidContent_ReturnsUnreadable()
    {
        await using var pdf = new MemoryStream(Encoding.ASCII.GetBytes("this is not a pdf"));

        var result = await _extractor.ExtractAsync(pdf, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<PdfReadResult.Failure>().Issue.Code.ShouldBe(IssueCodes.PdfUnreadable);
    }

    [Fact]
    public async Task ExtractAsync_EmptyStream_ReturnsUnreadable()
    {
        await using var pdf = new MemoryStream();

        var result = await _extractor.ExtractAsync(pdf, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<PdfReadResult.Failure>().Issue.Code.ShouldBe(IssueCodes.PdfUnreadable);
    }

    [Fact]
    public async Task ExtractAsync_CanceledToken_Throws()
    {
        await using var pdf = TestData.OpenSamplePdf();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => _extractor.ExtractAsync(pdf, cancellation.Token));
    }

    private static List<string> Lines(PageContent page) => [.. page.Text.Split('\n')];
}
