using FlightParser.Parsing;
using FlightParser.Pdf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FlightParser.DependencyInjection;

/// <summary>
/// Registers the flight plan parser in an <see cref="IServiceCollection"/>.
/// </summary>
public static class FlightParserServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IFlightPlanParser"/> as a singleton. Calling this method more than once has no further effect.
    /// </summary>
    /// <param name="services">The service collection to add the parser to.</param>
    /// <returns>The same service collection, to allow chaining.</returns>
    public static IServiceCollection AddFlightPlanParser(this IServiceCollection services)
    {
        services.TryAddSingleton<IFlightPlanParser>(provider =>
            new FlightPlanParser(
                provider.GetService<ILogger<FlightPlanParser>>() ?? NullLogger<FlightPlanParser>.Instance,
                new PdfPigTextExtractor()));

        return services;
    }
}
