using Microsoft.Extensions.Logging.Abstractions;

namespace FlightParser.Tests;

public sealed class FlightPlanParserFactoryTests
{
    [Fact]
    public void Create_WithoutLoggerFactory_CreatesParser()
    {
        var parser = FlightPlanParserFactory.Create();

        parser.ShouldNotBeNull();
    }

    [Fact]
    public void Create_WithLoggerFactory_CreatesParser()
    {
        var parser = FlightPlanParserFactory.Create(NullLoggerFactory.Instance);

        parser.ShouldNotBeNull();
    }
}
