using FlightParser.Parsing.Fields;

namespace FlightParser.Tests.Parsing.Fields;

public sealed class FieldValuesTests
{
    [Theory]
    [InlineData("0:48", 0, 48)]
    [InlineData("2:19", 2, 19)]
    [InlineData("12:05", 12, 5)]
    public void TryParseDuration_ValidValue_ReturnsDuration(string raw, int hours, int minutes)
    {
        FieldValues.TryParseDuration(raw, out var value).ShouldBeTrue();
        value.ShouldBe(new TimeSpan(hours, minutes, 0));
    }

    [Theory]
    [InlineData("0:60")]
    [InlineData("48")]
    [InlineData("0:4")]
    [InlineData("")]
    public void TryParseDuration_InvalidValue_ReturnsFalse(string raw)
    {
        FieldValues.TryParseDuration(raw, out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("1.7", 1.7)]
    [InlineData("12", 12)]
    public void TryParseDecimal_ValidValue_ReturnsValue(string raw, double expected)
    {
        FieldValues.TryParseDecimal(raw, out var value).ShouldBeTrue();
        value.ShouldBe((decimal)expected);
    }

    [Theory]
    [InlineData("1,7")]
    [InlineData("-1.7")]
    [InlineData("1.7t")]
    public void TryParseDecimal_InvalidValue_ReturnsFalse(string raw)
    {
        FieldValues.TryParseDecimal(raw, out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("34066", true)]
    [InlineData("-1", false)]
    [InlineData("34'066", false)]
    public void TryParseInteger_ReturnsExpectedResult(string raw, bool expected)
    {
        FieldValues.TryParseInteger(raw, out _).ShouldBe(expected);
    }

    [Theory]
    [InlineData("08:00", true)]
    [InlineData("23:59", true)]
    [InlineData("24:00", false)]
    [InlineData("8:00", false)]
    public void TryParseTimeOfDay_ReturnsExpectedResult(string raw, bool expected)
    {
        FieldValues.TryParseTimeOfDay(raw, out _).ShouldBe(expected);
    }
}
