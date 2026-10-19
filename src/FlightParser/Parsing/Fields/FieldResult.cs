namespace FlightParser.Parsing.Fields;

internal abstract record FieldResult<TValue>
{
    private FieldResult()
    {
    }

    public sealed record Found(TValue Value) : FieldResult<TValue>;

    public sealed record NotFound(string Reason) : FieldResult<TValue>;

    public sealed record Invalid(string RawText, string Reason) : FieldResult<TValue>;
}
