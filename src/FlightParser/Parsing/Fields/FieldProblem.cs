using FlightParser.Issues;

namespace FlightParser.Parsing.Fields;

internal sealed record FieldProblem(string Code, string Message, string? RawText)
{
    public static FieldProblem Missing(string message) =>
        new(IssueCodes.FieldMissing, message, RawText: null);

    public static FieldProblem InvalidFormat(string rawText, string message) =>
        new(IssueCodes.FieldInvalidFormat, message, rawText);
}
