using FlightParser.Issues;
using FlightParser.Parsing;
using FlightParser.Pdf;
using FlightParser.Tests.Parsing.Fields;

namespace FlightParser.Tests.Parsing;

public sealed class SectionParserTests
{
    private readonly SectionParser<FieldTests.TestBuilder> _parser =
        new("TestChapter", [new FieldTests.NumberField()]);

    [Fact]
    public void Parse_AllFieldsFound_ReturnsBuilderWithoutIssues()
    {
        var result = _parser.Parse(new PageContent(3, "NUM: 42", []));

        result.Builder.Number.ShouldBe(42);
        result.Issues.ShouldBeEmpty();
        result.PageNumber.ShouldBe(3);
    }

    [Fact]
    public void Parse_FieldMissing_ReportsIssueWithFullContext()
    {
        var result = _parser.Parse(new PageContent(3, "nothing", []));

        var issue = result.Issues.ShouldHaveSingleItem();
        issue.ShouldBe(issue with
        {
            Code = IssueCodes.FieldMissing,
            Chapter = "TestChapter",
            Field = "Number",
            PageNumber = 3,
        });
        issue.Message.ShouldStartWith("Number: ");
    }
}
