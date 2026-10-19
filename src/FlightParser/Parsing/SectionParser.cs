using FlightParser.Issues;
using FlightParser.Parsing.Fields;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Pdf;

namespace FlightParser.Parsing;

internal sealed class SectionParser<TBuilder>(string chapter, IReadOnlyList<IField<TBuilder>> fields)
    where TBuilder : new()
{
    public SectionParseResult<TBuilder> Parse(PageContent page)
    {
        var builder = new TBuilder();
        var issues = new List<ParseIssue>();

        foreach (var field in fields)
        {
            var problem = field.Extract(page, builder);
            if (problem is not null)
            {
                issues.Add(new ParseIssue(
                    problem.Code,
                    $"{field.Name}: {problem.Message}",
                    chapter,
                    field.Name,
                    page.PageNumber,
                    problem.RawText));
            }
        }

        return new SectionParseResult<TBuilder>(page.PageNumber, builder, issues);
    }
}
