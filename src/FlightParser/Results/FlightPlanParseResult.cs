using System.Diagnostics;
using FlightParser.Issues;
using FlightParser.Models;

namespace FlightParser.Results;

/// <summary>
/// The result of <see cref="IFlightPlanParser.ParseAsync"/>. It is either <see cref="Parsed"/> or <see cref="Rejected"/>;
/// no other subtypes exist.
/// </summary>
public abstract record FlightPlanParseResult
{
    private FlightPlanParseResult()
    {
    }

    /// <summary>
    /// Invokes the function that matches the actual result type and returns its value.
    /// </summary>
    /// <typeparam name="TResult">The type of the returned value.</typeparam>
    /// <param name="parsed">The function invoked for a <see cref="Parsed"/> result.</param>
    /// <param name="rejected">The function invoked for a <see cref="Rejected"/> result.</param>
    /// <returns>The value returned by the invoked function.</returns>
    public TResult Match<TResult>(Func<Parsed, TResult> parsed, Func<Rejected, TResult> rejected)
    {
        return this switch
        {
            Parsed result => parsed(result),
            Rejected result => rejected(result),
            _ => throw new UnreachableException(),
        };
    }

    /// <summary>
    /// The document was read and contains at least one Operational Flight Plan and one Crew Briefing.
    /// </summary>
    /// <param name="ValidFlights">Flights whose required fields were all extracted. They may carry warnings.</param>
    /// <param name="FailedFlights">Flights that were discarded, each with all reasons.</param>
    public sealed record Parsed(
        IReadOnlyList<FlightData> ValidFlights,
        IReadOnlyList<FailedFlight> FailedFlights) : FlightPlanParseResult
    {
        /// <summary>
        /// Gets a value indicating whether at least one flight was discarded.
        /// </summary>
        public bool HasFailedFlights => FailedFlights.Count > 0;
    }

    /// <summary>
    /// The document as a whole could not be processed, for example because it is corrupt,
    /// password protected, or contains no Operational Flight Plan or no Crew Briefing.
    /// </summary>
    /// <param name="Issues">One or more errors explaining why the document was rejected.</param>
    public sealed record Rejected(IReadOnlyList<ParseIssue> Issues) : FlightPlanParseResult;
}
