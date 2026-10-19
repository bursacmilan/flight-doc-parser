namespace FlightParser.Parsing.Fields.Builders;

internal sealed class OfpSectionBuilder
{
    public string? FlightNumber { get; set; }

    public string? AtcCallSign { get; set; }

    public DateOnly? Date { get; set; }

    public string? AircraftRegistration { get; set; }

    public string? DepartureAirport { get; set; }

    public string? DestinationAirport { get; set; }

    public string? Alternate1 { get; set; }

    public string? Alternate2 { get; set; }

    public TimeOnly? ScheduledDepartureUtc { get; set; }

    public TimeOnly? ScheduledArrivalUtc { get; set; }

    public int? ZeroFuelMassKg { get; set; }

    public TimeSpan? TimeToDestination { get; set; }

    public decimal? FuelToDestinationTons { get; set; }

    public TimeSpan? TimeToAlternate { get; set; }

    public decimal? FuelToAlternateTons { get; set; }

    public decimal? MinimumFuelRequiredTons { get; set; }

    public string? RouteFirstNavPoint { get; set; }

    public string? RouteLastNavPoint { get; set; }

    public int? GainLossUsdPerTon { get; set; }
}
