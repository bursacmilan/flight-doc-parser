using FlightParser.Parsing.Fields.Abstractions;

namespace FlightParser.Parsing.Fields;

internal static class FieldCatalog<TBuilder>
{
    public static IReadOnlyList<IField<TBuilder>> All { get; } = Discover();

    private static List<IField<TBuilder>> Discover() =>
        typeof(IField<TBuilder>).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IField<TBuilder>).IsAssignableFrom(type))
            .OrderBy(type => type.Name, StringComparer.Ordinal)
            .Select(type => (IField<TBuilder>)Activator.CreateInstance(type)!)
            .ToList();
}
