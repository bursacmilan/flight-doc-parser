using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using FlightParser.Pdf;

namespace FlightParser.Parsing.Fields.Abstractions;

internal abstract class RegexField<TBuilder, TValue> : Field<TBuilder, TValue>
{
    protected const string ValueGroup = "value";

    protected abstract Regex Pattern { get; }

    protected abstract string Label { get; }

    protected abstract bool TryConvert(string rawValue, [MaybeNullWhen(false)] out TValue value);

    protected sealed override FieldResult<TValue> ExtractValue(PageContent page)
    {
        var match = Pattern.Match(page.Text);
        if (!match.Success)
        {
            return new FieldResult<TValue>.NotFound($"Label '{Label}' not found.");
        }

        var rawValue = match.Groups[ValueGroup].Value;
        return TryConvert(rawValue, out var value)
            ? new FieldResult<TValue>.Found(value)
            : new FieldResult<TValue>.Invalid(match.Value, $"Value '{rawValue}' after '{Label}' is not in the expected format.");
    }
}
