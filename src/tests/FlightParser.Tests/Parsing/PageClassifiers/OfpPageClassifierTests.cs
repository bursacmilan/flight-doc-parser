using FlightParser.Parsing.PageClassifiers;
using FlightParser.Pdf;
using FlightParser.Tests.Pdf;
using FlightParser.Tests.TestSupport;

namespace FlightParser.Tests.Parsing.PageClassifiers;

public sealed class OfpPageClassifierTests(SamplePdfFixture sample) : IClassFixture<SamplePdfFixture>
{
    private readonly OfpPageClassifier _classifier = new();

    [Theory]
    [InlineData(5, true)]
    [InlineData(12, true)]
    [InlineData(19, true)]
    [InlineData(26, true)]
    [InlineData(1, false)]
    [InlineData(6, false)]
    [InlineData(85, false)]
    [InlineData(86, false)]
    public void IsMatch_FixturePage_RecognizesFirstOfpPageOnly(int pageNumber, bool expected)
    {
        _classifier.IsMatch(TestData.PageFromFixture(pageNumber)).ShouldBe(expected);
    }

    [Fact]
    public void IsMatch_SamplePdf_FindsExactlyTheFourOfps()
    {
        var matches = sample.Pages.Where(_classifier.IsMatch).Select(page => page.PageNumber);

        matches.ShouldBe([5, 12, 19, 26]);
    }

    [Fact]
    public void IsMatch_TitleWithoutFirstPageHeader_IsFalse()
    {
        var page = Page("Helvetic Airways - E-Jet Operational Flight Plan");

        _classifier.IsMatch(page).ShouldBeFalse();
    }

    [Fact]
    public void IsMatch_HeaderOfPageTen_IsFalse()
    {
        var page = Page("FMS IDENT=D6082 Log Nr.: 2893 Page 10 ZRH-MXP SWR612Q\nHelvetic Airways - E-Jet Operational Flight Plan");

        _classifier.IsMatch(page).ShouldBeFalse();
    }

    [Fact]
    public void IsMatch_HeaderWithoutSpaceAfterLogNumberLabel_IsTrue()
    {
        var page = Page("FMS IDENT=D6082 Log Nr.:2893 Page 1 ZRH-MXP SWR612Q\nHelvetic Airways - E-Jet Operational Flight Plan");

        _classifier.IsMatch(page).ShouldBeTrue();
    }

    private static PageContent Page(string text) => new(1, text, []);
}
