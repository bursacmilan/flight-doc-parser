using FlightParser.Parsing;
using FlightParser.Pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FlightParser;

/// <summary>
/// Creates <see cref="IFlightPlanParser"/> instances for callers that do not use dependency injection.
/// </summary>
public static class FlightPlanParserFactory
{
    /// <summary>
    /// Creates a new <see cref="IFlightPlanParser"/>.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used for diagnostic logging, or <see langword="null"/> to disable logging.</param>
    /// <returns>A stateless, thread-safe parser that can be reused for any number of documents.</returns>
    public static IFlightPlanParser Create(ILoggerFactory? loggerFactory = null) =>
        new FlightPlanParser((loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<FlightPlanParser>(), new PdfPigTextExtractor());
}
