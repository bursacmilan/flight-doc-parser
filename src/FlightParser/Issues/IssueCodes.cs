namespace FlightParser.Issues;

/// <summary>
/// The stable codes used in <see cref="ParseIssue.Code"/>. Callers can rely on them for branching and localization.
/// </summary>
public static class IssueCodes
{
    /// <summary>
    /// The PDF could not be read, for example because it is corrupt or not a PDF at all.
    /// </summary>
    public const string PdfUnreadable = "PDF_UNREADABLE";

    /// <summary>
    /// The PDF is password protected.
    /// </summary>
    public const string PdfPasswordProtected = "PDF_PASSWORD_PROTECTED";

    /// <summary>
    /// The document contains no Operational Flight Plan.
    /// </summary>
    public const string NoFlightPlanFound = "NO_FLIGHT_PLAN_FOUND";

    /// <summary>
    /// The document contains no Crew Briefing.
    /// </summary>
    public const string NoCrewBriefingFound = "NO_CREW_BRIEFING_FOUND";

    /// <summary>
    /// A required field was not found.
    /// </summary>
    public const string FieldMissing = "FIELD_MISSING";

    /// <summary>
    /// A field was found but its value has an unexpected format.
    /// </summary>
    public const string FieldInvalidFormat = "FIELD_INVALID_FORMAT";

    /// <summary>
    /// An Operational Flight Plan has no matching Crew Briefing.
    /// </summary>
    public const string CrewBriefingMissing = "CREW_BRIEFING_MISSING";

    /// <summary>
    /// A Crew Briefing has no matching Operational Flight Plan.
    /// </summary>
    public const string FlightPlanMissing = "FLIGHT_PLAN_MISSING";

    /// <summary>
    /// The same flight occurs more than once in the same chapter.
    /// </summary>
    public const string DuplicateFlight = "DUPLICATE_FLIGHT";
}
