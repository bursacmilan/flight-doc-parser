using FlightParser.Results;

namespace FlightParser;

/// <summary>
/// Extracts flight data from a flight planning PDF (crew briefing package) that contains
/// Operational Flight Plans (OFP) and Crew Briefings for one or more flights.
/// </summary>
/// <remarks>
/// Implementations are stateless and thread-safe and can be registered as a singleton.
/// Expected problems in the document are reported through the returned
/// <see cref="FlightPlanParseResult"/> and never as exceptions.
/// </remarks>
public interface IFlightPlanParser
{
    /// <summary>
    /// Parses the given PDF document and returns the flights it contains.
    /// </summary>
    /// <param name="pdf">
    /// The PDF document. The stream is owned by the caller: it is neither disposed nor required to be seekable.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <see cref="FlightPlanParseResult.Parsed"/> if the document could be read and contains at least one
    /// Operational Flight Plan and one Crew Briefing, otherwise <see cref="FlightPlanParseResult.Rejected"/>.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    Task<FlightPlanParseResult> ParseAsync(Stream pdf, CancellationToken cancellationToken = default);
}
