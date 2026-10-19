using FlightParser.Parsing.Fields.Validators;

namespace FlightParser.Parsing.Fields.Abstractions;

internal abstract class AirportField<TBuilder> : RegexField<TBuilder, string>
{
    protected override bool TryConvert(string rawValue, out string value)
    {
        value = rawValue;
        return IcaoValidator.IsValid(rawValue);
    }
}
