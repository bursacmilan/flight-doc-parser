using FlightParser.Issues;
using FlightParser.Parsing;
using FlightParser.Parsing.Fields;
using FlightParser.Parsing.Fields.Builders;
using FlightParser.Tests.Pdf;

namespace FlightParser.Tests.Parsing.Fields.Crew;

public sealed class CrewBriefingFieldsTests(SamplePdfFixture sample) : IClassFixture<SamplePdfFixture>
{
    private static readonly string[] CrewFunctions = ["CMD", "COP", "CAB", "CAB", "SEN"];

    private static readonly string[] FunctionColumnWords = ["Commander", "Copilot", "Senior", "Cabin", "Attendant"];

    private static readonly int[] NameWordCountsLx1072And1073 = [2, 3, 2, 2, 2];

    private static readonly int[] NameWordCountsLx1612And1613 = [2, 2, 3, 2, 3];

    private readonly SectionParser<CrewBriefingSectionBuilder> _parser = new(Chapters.CrewBriefing, FieldCatalog<CrewBriefingSectionBuilder>.All);

    [Fact]
    public void FieldCatalog_FindsAllCrewBriefingFields()
    {
        FieldCatalog<CrewBriefingSectionBuilder>.All.Select(field => field.Name).Order(StringComparer.Ordinal).ShouldBe(
            ["Crew", "Date", "DryOperatingIndex", "DryOperatingWeight", "FlightNumber", "Passengers"]);
    }

    [Fact]
    public void Parse_Lx1073_ExtractsAllFields()
    {
        var result = Parse(85);

        result.Issues.ShouldBeEmpty();
        AssertBuilder(result.Builder, "LX1073", 1, 26, 28916, 68.7m, NameWordCountsLx1072And1073);
    }

    [Fact]
    public void Parse_Lx1072_ExtractsAllFields()
    {
        var result = Parse(93);

        result.Issues.ShouldBeEmpty();
        AssertBuilder(result.Builder, "LX1072", 9, 31, 28916, 68.7m, NameWordCountsLx1072And1073);
    }

    [Theory]
    [InlineData(89, "LX1613", 18, 50, 28822)]
    [InlineData(97, "LX1612", 9, 42, 28822)]
    public void Parse_DryOperatingIndexZero_ExtractsAllFieldsWithoutIssues(int pageNumber, string flightNumber, int business, int economy, int dryOperatingWeight)
    {
        var result = Parse(pageNumber);

        result.Issues.ShouldBeEmpty();
        AssertBuilder(result.Builder, flightNumber, business, economy, dryOperatingWeight, 0m, NameWordCountsLx1612And1613);
    }

    [Theory]
    [InlineData("19.Mar.2024", "19.Mrz.2024", "Date", IssueCodes.FieldInvalidFormat)]
    [InlineData("19.Mar.2024 DEP", "Date DEP", "Date", IssueCodes.FieldMissing)]
    [InlineData("LX1073 TIME", "LX-1073 TIME", "FlightNumber", IssueCodes.FieldInvalidFormat)]
    [InlineData("1/26", "1-26", "Passengers", IssueCodes.FieldInvalidFormat)]
    [InlineData("FRA ZRH", "FRANKFURT ZRH", "Passengers", IssueCodes.FieldMissing)]
    [InlineData("DOW: 28916kg", "DOW: 28.916kg", "DryOperatingWeight", IssueCodes.FieldInvalidFormat)]
    [InlineData("DOW: 28916kg", "DOX: 28916kg", "DryOperatingWeight", IssueCodes.FieldMissing)]
    [InlineData("DOI: 68.7", "DOI: 68,7", "DryOperatingIndex", IssueCodes.FieldInvalidFormat)]
    [InlineData("DOI: 68.7", "DOX: 68.7", "DryOperatingIndex", IssueCodes.FieldMissing)]
    public void Parse_CorruptedField_ReportsIssueForThatField(string search, string replacement, string field, string expectedCode)
    {
        var page = sample.Page(85);
        page.Text.ShouldContain(search);

        var result = _parser.Parse(page with { Text = page.Text.Replace(search, replacement, StringComparison.Ordinal) });

        var issue = result.Issues.Where(i => i.Field == field).ShouldHaveSingleItem();
        issue.Code.ShouldBe(expectedCode);
        issue.Chapter.ShouldBe(Chapters.CrewBriefing);
        issue.PageNumber.ShouldBe(85);
    }

    [Fact]
    public void Parse_PageWithoutTableLines_ReportsCrewMissing()
    {
        var page = sample.Page(85) with { Lines = [] };

        var result = _parser.Parse(page);

        var issue = result.Issues.ShouldHaveSingleItem();
        issue.Field.ShouldBe("Crew");
        issue.Code.ShouldBe(IssueCodes.FieldMissing);
    }

    private SectionParseResult<CrewBriefingSectionBuilder> Parse(int pageNumber) => _parser.Parse(sample.Page(pageNumber));

    private static void AssertBuilder(
        CrewBriefingSectionBuilder builder,
        string flightNumber,
        int business,
        int economy,
        int dryOperatingWeightKg,
        decimal dryOperatingIndex,
        int[] nameWordCounts)
    {
        builder.ShouldSatisfyAllConditions(
            () => builder.FlightNumber.ShouldBe(flightNumber),
            () => builder.Date.ShouldBe(new DateOnly(2024, 3, 19)),
            () => builder.PassengersBusiness.ShouldBe(business),
            () => builder.PassengersEconomy.ShouldBe(economy),
            () => builder.DryOperatingWeightKg.ShouldBe(dryOperatingWeightKg),
            () => builder.DryOperatingIndex.ShouldBe(dryOperatingIndex),
            () => builder.Crew.ShouldNotBeNull().Select(member => member.Function).ShouldBe(CrewFunctions),
            () => builder.Crew.ShouldNotBeNull().Select(member => member.Name.Split(' ').Length).ShouldBe(nameWordCounts),
            () => builder.Crew.ShouldNotBeNull().ShouldAllBe(member => !member.Name.Split(' ').Intersect(FunctionColumnWords).Any()));
    }
}
