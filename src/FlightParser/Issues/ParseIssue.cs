namespace FlightParser.Issues;

/// <summary>
/// A problem found while parsing the document, with enough context to understand and report it.
/// </summary>
/// <param name="Code">A stable identifier of the kind of issue. See <see cref="IssueCodes"/>.</param>
/// <param name="Message">A human-readable description in English, intended for logs and diagnostics.</param>
/// <param name="Chapter">The chapter in which the issue was found (<c>OFP</c> or <c>CrewBriefing</c>), or <see langword="null"/> for document level issues.</param>
/// <param name="Field">The name of the affected field, e.g. <c>ZeroFuelMass</c>, or <see langword="null"/> if the issue is not about a single field.</param>
/// <param name="PageNumber">The 1-based page number in the PDF, or <see langword="null"/> if the issue is not tied to a page.</param>
/// <param name="RawText">The text snippet that caused the issue, or <see langword="null"/> if there is none.</param>
public sealed record ParseIssue(
    string Code,
    string Message,
    string? Chapter,
    string? Field,
    int? PageNumber,
    string? RawText);
