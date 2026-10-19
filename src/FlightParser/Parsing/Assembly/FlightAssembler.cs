using System.Diagnostics;
using System.Runtime.CompilerServices;
using FlightParser.Issues;
using FlightParser.Models;
using FlightParser.Parsing.Fields.Builders;
using FlightParser.Results;

namespace FlightParser.Parsing.Assembly;

internal static class FlightAssembler
{
    public static FlightPlanParseResult.Parsed Assemble(
        IReadOnlyList<SectionParseResult<OfpSectionBuilder>> flightPlans,
        IReadOnlyList<SectionParseResult<CrewBriefingSectionBuilder>> crewBriefings)
    {
        var validFlights = new List<FlightData>();
        var failedFlights = new List<FailedFlight>();

        var flightPlansByKey = GroupByKey(flightPlans, plan => FlightKey.From(plan.Builder.FlightNumber, plan.Builder.Date), failedFlights);
        var crewBriefingsByKey = GroupByKey(crewBriefings, briefing => FlightKey.From(briefing.Builder.FlightNumber, briefing.Builder.Date), failedFlights);

        foreach (var (key, plans) in flightPlansByKey)
        {
            crewBriefingsByKey.Remove(key, out var briefings);
            briefings ??= [];

            var issues = plans.SelectMany(plan => plan.Issues).Concat(briefings.SelectMany(briefing => briefing.Issues)).ToList();
            issues.AddRange(DuplicateIssues(key, Chapters.Ofp, plans.Select(plan => plan.PageNumber)));
            issues.AddRange(DuplicateIssues(key, Chapters.CrewBriefing, briefings.Select(briefing => briefing.PageNumber)));

            if (briefings.Count == 0)
            {
                issues.Add(Error(IssueCodes.CrewBriefingMissing, Chapters.CrewBriefing, $"No Crew Briefing found for flight {key}.", pageNumber: null));
            }

            if (issues.Count > 0)
            {
                failedFlights.Add(new FailedFlight(key.FlightNumber, key.Date, issues));
            }
            else
            {
                validFlights.Add(Build(plans[0].Builder, briefings[0].Builder));
            }
        }

        foreach (var (key, briefings) in crewBriefingsByKey)
        {
            var issues = briefings.SelectMany(briefing => briefing.Issues).ToList();
            issues.AddRange(DuplicateIssues(key, Chapters.CrewBriefing, briefings.Select(briefing => briefing.PageNumber)));
            issues.Add(Error(IssueCodes.FlightPlanMissing, Chapters.Ofp, $"No Operational Flight Plan found for flight {key}.", briefings[0].PageNumber));

            failedFlights.Add(new FailedFlight(key.FlightNumber, key.Date, issues));
        }

        return new FlightPlanParseResult.Parsed(validFlights, failedFlights);
    }

    private static OrderedDictionary<FlightKey, List<SectionParseResult<TBuilder>>> GroupByKey<TBuilder>(
        IReadOnlyList<SectionParseResult<TBuilder>> sections,
        Func<SectionParseResult<TBuilder>, FlightKey?> keyOf,
        List<FailedFlight> failedFlights)
        where TBuilder : class
    {
        var groups = new OrderedDictionary<FlightKey, List<SectionParseResult<TBuilder>>>();

        foreach (var section in sections)
        {
            if (keyOf(section) is not { } key)
            {
                failedFlights.Add(Unidentified(section));
                continue;
            }

            if (!groups.TryGetValue(key, out var group))
            {
                group = [];
                groups.Add(key, group);
            }

            group.Add(section);
        }

        return groups;
    }

    private static FailedFlight Unidentified<TBuilder>(SectionParseResult<TBuilder> section) =>
        section.Builder switch
        {
            OfpSectionBuilder plan => new FailedFlight(plan.FlightNumber, plan.Date, section.Issues),
            CrewBriefingSectionBuilder briefing => new FailedFlight(briefing.FlightNumber, briefing.Date, section.Issues),
            _ => throw new UnreachableException(),
        };

    private static IEnumerable<ParseIssue> DuplicateIssues(FlightKey key, string chapter, IEnumerable<int> pageNumbers) =>
        pageNumbers.Skip(1).Select(pageNumber =>
            Error(IssueCodes.DuplicateFlight, chapter, $"Flight {key} occurs more than once in the {chapter} chapter.", pageNumber));

    private static ParseIssue Error(string code, string chapter, string message, int? pageNumber) =>
        new(code, message, chapter, Field: null, pageNumber, RawText: null);

    private static FlightData Build(OfpSectionBuilder plan, CrewBriefingSectionBuilder briefing) =>
        new(
            FlightNumber: Required(plan.FlightNumber),
            AtcCallSign: Required(plan.AtcCallSign),
            Date: Required(plan.Date),
            AircraftRegistration: Required(plan.AircraftRegistration),
            DepartureAirport: Required(plan.DepartureAirport),
            DestinationAirport: Required(plan.DestinationAirport),
            Alternate1: Required(plan.Alternate1),
            Alternate2: plan.Alternate2,
            ScheduledDepartureUtc: Required(plan.ScheduledDepartureUtc),
            ScheduledArrivalUtc: Required(plan.ScheduledArrivalUtc),
            ZeroFuelMassKg: Required(plan.ZeroFuelMassKg),
            TimeToDestination: Required(plan.TimeToDestination),
            FuelToDestinationTons: Required(plan.FuelToDestinationTons),
            TimeToAlternate: Required(plan.TimeToAlternate),
            FuelToAlternateTons: Required(plan.FuelToAlternateTons),
            MinimumFuelRequiredTons: Required(plan.MinimumFuelRequiredTons),
            RouteFirstNavPoint: Required(plan.RouteFirstNavPoint),
            RouteLastNavPoint: Required(plan.RouteLastNavPoint),
            GainLossUsdPerTon: Required(plan.GainLossUsdPerTon),
            PassengersBusiness: Required(briefing.PassengersBusiness),
            PassengersEconomy: Required(briefing.PassengersEconomy),
            DryOperatingWeightKg: Required(briefing.DryOperatingWeightKg),
            DryOperatingIndex: Required(briefing.DryOperatingIndex),
            Crew: Required(briefing.Crew));

    private static T Required<T>(T? value, [CallerArgumentExpression(nameof(value))] string expression = "")
        where T : struct =>
        value ?? throw new UnreachableException($"{expression} is not set although the section has no errors.");

    private static T Required<T>(T? value, [CallerArgumentExpression(nameof(value))] string expression = "")
        where T : class =>
        value ?? throw new UnreachableException($"{expression} is not set although the section has no errors.");
}
