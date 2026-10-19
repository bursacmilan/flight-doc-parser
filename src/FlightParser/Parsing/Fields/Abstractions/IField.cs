using FlightParser.Pdf;

namespace FlightParser.Parsing.Fields.Abstractions;

internal interface IField<in TBuilder>
{
    string Name { get; }

    FieldProblem? Extract(PageContent page, TBuilder builder);
}
