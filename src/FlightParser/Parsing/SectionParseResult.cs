using FlightParser.Issues;

namespace FlightParser.Parsing;

internal sealed record SectionParseResult<TBuilder>(int PageNumber, TBuilder Builder, IReadOnlyList<ParseIssue> Issues);
