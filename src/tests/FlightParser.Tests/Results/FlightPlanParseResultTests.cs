using FlightParser.Issues;
using FlightParser.Results;

namespace FlightParser.Tests.Results;

public sealed class FlightPlanParseResultTests
{
    private static readonly ParseIssue SampleError =
        new(IssueCodes.FieldMissing, "ZFM not found.", "OFP", "ZeroFuelMass", 5, null);

    [Fact]
    public void Match_OnParsed_InvokesParsedFunction()
    {
        FlightPlanParseResult result = new FlightPlanParseResult.Parsed([], []);

        var outcome = result.Match(parsed: _ => "parsed", rejected: _ => "rejected");

        outcome.ShouldBe("parsed");
    }

    [Fact]
    public void Match_OnRejected_InvokesRejectedFunction()
    {
        FlightPlanParseResult result = new FlightPlanParseResult.Rejected([SampleError]);

        var outcome = result.Match(parsed: _ => "parsed", rejected: r => r.Issues[0].Code);

        outcome.ShouldBe(IssueCodes.FieldMissing);
    }

    [Fact]
    public void HasFailedFlights_WithoutFailedFlights_IsFalse()
    {
        var result = new FlightPlanParseResult.Parsed([], []);

        result.HasFailedFlights.ShouldBeFalse();
    }

    [Fact]
    public void HasFailedFlights_WithFailedFlight_IsTrue()
    {
        var failed = new FailedFlight("LX1612", new DateOnly(2024, 3, 19), [SampleError]);

        var result = new FlightPlanParseResult.Parsed([], [failed]);

        result.HasFailedFlights.ShouldBeTrue();
    }
}
