using FlightParser.Issues;
using FlightParser.Parsing;
using FlightParser.Pdf;
using FlightParser.Results;
using FlightParser.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;

namespace FlightParser.Tests.Parsing;

public sealed class FlightPlanParserTests
{
    [Fact]
    public async Task ParseAsync_UnreadablePdf_RejectsWithExtractorIssue()
    {
        var issue = new ParseIssue(IssueCodes.PdfUnreadable, "broken", null, null, null, null);
        var parser = Parser(new PdfReadResult.Failure(issue));

        var result = await parser.ParseAsync(Stream.Null, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<FlightPlanParseResult.Rejected>().Issues.ShouldBe([issue]);
    }

    [Fact]
    public async Task ParseAsync_NoFlightPlanPage_RejectsWithNoFlightPlanFound()
    {
        var parser = Parser(new PdfReadResult.Success([TestData.PageFromFixture(1), TestData.PageFromFixture(6)]));

        var result = await parser.ParseAsync(Stream.Null, TestContext.Current.CancellationToken);

        var issue = result.ShouldBeOfType<FlightPlanParseResult.Rejected>().Issues.ShouldHaveSingleItem();
        issue.Code.ShouldBe(IssueCodes.NoFlightPlanFound);
    }

    [Fact]
    public async Task ParseAsync_NoCrewBriefingPage_RejectsWithNoCrewBriefingFound()
    {
        var parser = Parser(new PdfReadResult.Success([TestData.PageFromFixture(1), TestData.PageFromFixture(5), TestData.PageFromFixture(86)]));

        var result = await parser.ParseAsync(Stream.Null, TestContext.Current.CancellationToken);

        var issue = result.ShouldBeOfType<FlightPlanParseResult.Rejected>().Issues.ShouldHaveSingleItem();
        issue.Code.ShouldBe(IssueCodes.NoCrewBriefingFound);
        issue.Chapter.ShouldBe(Chapters.CrewBriefing);
    }

    [Fact]
    public async Task ParseAsync_SingleFlightWithoutCrewBriefing_ReturnsFailedFlightForIt()
    {
        var parser = Parser(new PdfReadResult.Success([TestData.PageFromFixture(1), TestData.PageFromFixture(5), TestData.PageFromFixture(6), TestData.PageFromFixture(12), TestData.PageFromFixture(86), TestData.PageFromFixture(97)]));

        var result = await parser.ParseAsync(Stream.Null, TestContext.Current.CancellationToken);

        var parsed = result.ShouldBeOfType<FlightPlanParseResult.Parsed>();
        var lx1613 = parsed.FailedFlights.Where(flight => flight.FlightNumber == "LX1613").ShouldHaveSingleItem();
        lx1613.Issues.ShouldHaveSingleItem().Code.ShouldBe(IssueCodes.CrewBriefingMissing);
        parsed.FailedFlights.Select(flight => flight.FlightNumber).ShouldBe(["LX1612", "LX1613"]);
    }

    [Fact]
    public async Task ParseAsync_IgnoresPagesThatAreNeitherFlightPlanNorCrewBriefing()
    {
        var parser = Parser(new PdfReadResult.Success([TestData.PageFromFixture(1), TestData.PageFromFixture(6), TestData.PageFromFixture(86), TestData.PageFromFixture(5), TestData.PageFromFixture(97)]));

        var result = await parser.ParseAsync(Stream.Null, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<FlightPlanParseResult.Parsed>().FailedFlights.ShouldHaveSingleItem().FlightNumber.ShouldBe("LX1612");
    }

    private static FlightPlanParser Parser(PdfReadResult readResult) =>
        new(NullLogger<FlightPlanParser>.Instance, new FakePdfTextExtractor(readResult));
}
