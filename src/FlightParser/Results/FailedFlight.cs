using FlightParser.Issues;

namespace FlightParser.Results;

/// <summary>
/// A flight that was found in the document but discarded because required data is missing or invalid.
/// </summary>
/// <param name="FlightNumber">The flight number, or <see langword="null"/> if it could not be determined.</param>
/// <param name="Date">The date of the flight, or <see langword="null"/> if it could not be determined.</param>
/// <param name="Issues">All issues found for this flight. Contains at least one error.</param>
public sealed record FailedFlight(string? FlightNumber, DateOnly? Date, IReadOnlyList<ParseIssue> Issues);
