using FlightParser.Models;

namespace FlightParser.Parsing.Fields.Builders;

internal sealed class CrewBriefingSectionBuilder
{
    public string? FlightNumber { get; set; }

    public DateOnly? Date { get; set; }

    public int? PassengersBusiness { get; set; }

    public int? PassengersEconomy { get; set; }

    public int? DryOperatingWeightKg { get; set; }

    public decimal? DryOperatingIndex { get; set; }

    public IReadOnlyList<CrewMember>? Crew { get; set; }
}
