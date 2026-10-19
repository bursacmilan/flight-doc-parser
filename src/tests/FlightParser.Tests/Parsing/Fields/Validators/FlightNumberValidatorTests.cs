using FlightParser.Parsing.Fields.Validators;

namespace FlightParser.Tests.Parsing.Fields.Validators;

public sealed class FlightNumberValidatorTests
{
    [Theory]
    [InlineData("LX1612", true)]
    [InlineData("LX758", true)]
    [InlineData("U21234A", true)]
    [InlineData("2L123", true)]
    [InlineData("LX-1612", false)]
    [InlineData("LX", false)]
    [InlineData("lx1612", false)]
    [InlineData("TIME", false)]
    [InlineData("", false)]
    public void IsValid_ReturnsExpectedResult(string flightNumber, bool expected)
    {
        FlightNumberValidator.IsValid(flightNumber).ShouldBe(expected);
    }
}
