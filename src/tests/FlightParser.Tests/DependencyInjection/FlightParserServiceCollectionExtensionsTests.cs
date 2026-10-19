using FlightParser.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace FlightParser.Tests.DependencyInjection;

public sealed class FlightParserServiceCollectionExtensionsTests
{
    [Fact]
    public void AddFlightPlanParser_WithoutLogging_ResolvesSingleton()
    {
        using var provider = new ServiceCollection().AddFlightPlanParser().BuildServiceProvider();

        var first = provider.GetRequiredService<IFlightPlanParser>();
        var second = provider.GetRequiredService<IFlightPlanParser>();

        first.ShouldBeSameAs(second);
    }

    [Fact]
    public void AddFlightPlanParser_WithLogging_ResolvesParser()
    {
        using var provider = new ServiceCollection()
            .AddLogging()
            .AddFlightPlanParser()
            .BuildServiceProvider();

        provider.GetRequiredService<IFlightPlanParser>().ShouldNotBeNull();
    }

    [Fact]
    public void AddFlightPlanParser_CalledTwice_RegistersOnce()
    {
        var services = new ServiceCollection().AddFlightPlanParser().AddFlightPlanParser();

        services.Count(d => d.ServiceType == typeof(IFlightPlanParser)).ShouldBe(1);
    }
}
