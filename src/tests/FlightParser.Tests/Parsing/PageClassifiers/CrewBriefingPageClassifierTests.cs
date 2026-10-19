using FlightParser.Parsing.PageClassifiers;
using FlightParser.Pdf;
using FlightParser.Tests.Pdf;
using FlightParser.Tests.TestSupport;

namespace FlightParser.Tests.Parsing.PageClassifiers;

public sealed class CrewBriefingPageClassifierTests(SamplePdfFixture sample) : IClassFixture<SamplePdfFixture>
{
    private readonly CrewBriefingPageClassifier _classifier = new();

    [Theory]
    [InlineData(85, true)]
    [InlineData(89, true)]
    [InlineData(93, true)]
    [InlineData(97, true)]
    [InlineData(1, false)]
    [InlineData(5, false)]
    [InlineData(6, false)]
    [InlineData(86, false)]
    public void IsMatch_FixturePage_RecognizesFirstCrewBriefingPageOnly(int pageNumber, bool expected)
    {
        _classifier.IsMatch(TestData.PageFromFixture(pageNumber)).ShouldBe(expected);
    }

    [Fact]
    public void IsMatch_SamplePdf_FindsExactlyTheFourCrewBriefings()
    {
        var matches = sample.Pages.Where(_classifier.IsMatch).Select(page => page.PageNumber);

        matches.ShouldBe([85, 89, 93, 97]);
    }

    [Fact]
    public void IsMatch_TitleWithoutFirstPageFooter_IsFalse()
    {
        var page = Page("Flight Assignment / Flight Crew Briefing\nPage 2 of 4 19.Mar.2024 08:05:05");

        _classifier.IsMatch(page).ShouldBeFalse();
    }

    [Fact]
    public void IsMatch_FooterOfPageTen_IsFalse()
    {
        var page = Page("Flight Assignment / Flight Crew Briefing\nPage 10 of 12 19.Mar.2024 08:05:05");

        _classifier.IsMatch(page).ShouldBeFalse();
    }

    [Fact]
    public void IsMatch_FirstPageFooterWithoutTitle_IsFalse()
    {
        var page = Page("Some other document\nPage 1 of 4");

        _classifier.IsMatch(page).ShouldBeFalse();
    }

    private static PageContent Page(string text) => new(1, text, []);
}
