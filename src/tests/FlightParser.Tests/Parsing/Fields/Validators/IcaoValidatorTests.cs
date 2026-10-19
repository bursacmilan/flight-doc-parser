using FlightParser.Parsing.Fields.Validators;

namespace FlightParser.Tests.Parsing.Fields.Validators;

public sealed class IcaoValidatorTests
{
    [Theory]
    [InlineData("LSZH", true)]
    [InlineData("EDDF", true)]
    [InlineData("LIM", false)]
    [InlineData("LIMCX", false)]
    [InlineData("lszh", false)]
    [InlineData("LIM1", false)]
    [InlineData("CTOT:", false)]
    [InlineData("", false)]
    public void IsValid_ReturnsExpectedResult(string code, bool expected)
    {
        IcaoValidator.IsValid(code).ShouldBe(expected);
    }
}
