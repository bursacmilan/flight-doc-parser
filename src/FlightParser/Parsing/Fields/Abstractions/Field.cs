using System.Diagnostics;
using FlightParser.Pdf;

namespace FlightParser.Parsing.Fields.Abstractions;

internal abstract class Field<TBuilder, TValue> : IField<TBuilder>
{
    public abstract string Name { get; }

    protected virtual bool IsRequired => true;

    public FieldProblem? Extract(PageContent page, TBuilder builder) =>
        ExtractValue(page) switch
        {
            FieldResult<TValue>.Found found => ApplyFound(builder, found.Value),
            FieldResult<TValue>.NotFound notFound => IsRequired ? FieldProblem.Missing(notFound.Reason) : null,
            FieldResult<TValue>.Invalid invalid => FieldProblem.InvalidFormat(invalid.RawText, invalid.Reason),
            _ => throw new UnreachableException(),
        };

    protected abstract FieldResult<TValue> ExtractValue(PageContent page);

    protected abstract void Apply(TBuilder builder, TValue value);

    private FieldProblem? ApplyFound(TBuilder builder, TValue value)
    {
        Apply(builder, value);
        return null;
    }
}
