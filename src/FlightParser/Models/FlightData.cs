using FlightParser.Issues;

namespace FlightParser.Models;

/// <summary>
/// The data of one flight, combined from its Operational Flight Plan (OFP) and its Crew Briefing.
/// </summary>
/// <param name="FlightNumber">The flight number, e.g. <c>LX1612</c>.</param>
/// <param name="AtcCallSign">The ATC call sign, e.g. <c>SWR612Q</c>.</param>
/// <param name="Date">The date of the flight.</param>
/// <param name="AircraftRegistration">The aircraft registration as printed in the OFP, e.g. <c>HBJVY</c>.</param>
/// <param name="DepartureAirport">The ICAO code of the departure airport, e.g. <c>LSZH</c>.</param>
/// <param name="DestinationAirport">The ICAO code of the destination airport, e.g. <c>LIMC</c>.</param>
/// <param name="Alternate1">The ICAO code of the first alternate airport, e.g. <c>LIML</c>.</param>
/// <param name="Alternate2">The ICAO code of the second alternate airport, or <see langword="null"/> if there is none.</param>
/// <param name="ScheduledDepartureUtc">The scheduled time of departure (STD) in UTC.</param>
/// <param name="ScheduledArrivalUtc">The scheduled time of arrival (STA) in UTC.</param>
/// <param name="ZeroFuelMassKg">The zero fuel mass (ZFM) in kilograms.</param>
/// <param name="TimeToDestination">The planned flight time to the destination.</param>
/// <param name="FuelToDestinationTons">The planned fuel to the destination in metric tons.</param>
/// <param name="TimeToAlternate">The planned flight time from the destination to the first alternate.</param>
/// <param name="FuelToAlternateTons">The planned fuel from the destination to the first alternate in metric tons.</param>
/// <param name="MinimumFuelRequiredTons">The minimum fuel required in metric tons.</param>
/// <param name="RouteFirstNavPoint">The first navigation point of the ATC route to the destination, e.g. <c>VEBIT</c>.</param>
/// <param name="RouteLastNavPoint">The last navigation point of the ATC route to the destination, e.g. <c>RIXUV</c>.</param>
/// <param name="GainLossUsdPerTon">The fuel price gain (positive) or loss (negative) in US dollars per metric ton.</param>
/// <param name="PassengersBusiness">The number of passengers in business class (C).</param>
/// <param name="PassengersEconomy">The number of passengers in economy class (Y).</param>
/// <param name="DryOperatingWeightKg">The dry operating weight (DOW) in kilograms.</param>
/// <param name="DryOperatingIndex">The dry operating index (DOI).</param>
/// <param name="Crew">The operating crew members in the order of the Crew Briefing.</param>
public sealed record FlightData(
    string FlightNumber,
    string AtcCallSign,
    DateOnly Date,
    string AircraftRegistration,
    string DepartureAirport,
    string DestinationAirport,
    string Alternate1,
    string? Alternate2,
    TimeOnly ScheduledDepartureUtc,
    TimeOnly ScheduledArrivalUtc,
    int ZeroFuelMassKg,
    TimeSpan TimeToDestination,
    decimal FuelToDestinationTons,
    TimeSpan TimeToAlternate,
    decimal FuelToAlternateTons,
    decimal MinimumFuelRequiredTons,
    string RouteFirstNavPoint,
    string RouteLastNavPoint,
    int GainLossUsdPerTon,
    int PassengersBusiness,
    int PassengersEconomy,
    int DryOperatingWeightKg,
    decimal DryOperatingIndex,
    IReadOnlyList<CrewMember> Crew);
