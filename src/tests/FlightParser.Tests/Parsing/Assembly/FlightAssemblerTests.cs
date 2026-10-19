using FlightParser.Issues;
using FlightParser.Models;
using FlightParser.Parsing;
using FlightParser.Parsing.Assembly;
using FlightParser.Parsing.Fields;
using FlightParser.Parsing.Fields.Builders;
using FlightParser.Tests.Pdf;

namespace FlightParser.Tests.Parsing.Assembly;

public sealed class FlightAssemblerTests(SamplePdfFixture sample) : IClassFixture<SamplePdfFixture>
{
    private static readonly DateOnly March19 = new(2024, 3, 19);
    private static readonly int[] FlightPlanPages = [5, 12, 19, 26];
    private static readonly int[] CrewBriefingPages = [85, 89, 93, 97];

    [Fact]
    public void Assemble_MatchingSections_CombinesThemIntoOneFlight()
    {
        var result = FlightAssembler.Assemble([Plan("LX1612", page: 5)], [Briefing("LX1612", page: 97)]);

        result.FailedFlights.ShouldBeEmpty();
        var flight = result.ValidFlights.ShouldHaveSingleItem();
        flight.FlightNumber.ShouldBe("LX1612");
        flight.ZeroFuelMassKg.ShouldBe(34066);
        flight.PassengersEconomy.ShouldBe(42);
        flight.Crew.ShouldHaveSingleItem().Name.ShouldBe("Max Mustermann");
    }

    [Fact]
    public void Assemble_ShuffledChapterOrder_MatchesByKeyAndKeepsFlightPlanOrder()
    {
        var result = FlightAssembler.Assemble(
            [Plan("LX1612", page: 5), Plan("LX1073", page: 26)],
            [Briefing("LX1073", page: 85), Briefing("LX1612", page: 97)]);

        result.FailedFlights.ShouldBeEmpty();
        result.ValidFlights.Select(flight => flight.FlightNumber).ShouldBe(["LX1612", "LX1073"]);
    }

    [Fact]
    public void Assemble_SameFlightNumberOnOtherDate_IsNotMatched()
    {
        var result = FlightAssembler.Assemble([Plan("LX1612", page: 5)], [Briefing("LX1612", page: 97, date: March19.AddDays(1))]);

        result.ValidFlights.ShouldBeEmpty();
        result.FailedFlights.Select(flight => flight.Issues[^1].Code).ShouldBe([IssueCodes.CrewBriefingMissing, IssueCodes.FlightPlanMissing]);
    }

    [Fact]
    public void Assemble_FlightPlanWithoutCrewBriefing_FailsWithCrewBriefingMissing()
    {
        var result = FlightAssembler.Assemble([Plan("LX1612", page: 5)], []);

        var failed = result.FailedFlights.ShouldHaveSingleItem();
        failed.FlightNumber.ShouldBe("LX1612");
        failed.Date.ShouldBe(March19);
        failed.Issues.ShouldHaveSingleItem().Code.ShouldBe(IssueCodes.CrewBriefingMissing);
    }

    [Fact]
    public void Assemble_CrewBriefingWithoutFlightPlan_FailsWithFlightPlanMissing()
    {
        var result = FlightAssembler.Assemble([], [Briefing("LX1612", page: 97)]);

        var issue = result.FailedFlights.ShouldHaveSingleItem().Issues.ShouldHaveSingleItem();
        issue.Code.ShouldBe(IssueCodes.FlightPlanMissing);
        issue.PageNumber.ShouldBe(97);
    }

    [Fact]
    public void Assemble_SectionWithError_FailsWithIssuesOfBothSections()
    {
        var planError = Issue(IssueCodes.FieldInvalidFormat, Chapters.Ofp);
        var briefingError = Issue(IssueCodes.FieldMissing, Chapters.CrewBriefing);

        var result = FlightAssembler.Assemble([Plan("LX1612", page: 5, planError)], [Briefing("LX1612", page: 97, issues: briefingError)]);

        result.ValidFlights.ShouldBeEmpty();
        result.FailedFlights.ShouldHaveSingleItem().Issues.ShouldBe([planError, briefingError]);
    }

    [Fact]
    public void Assemble_DuplicateFlightPlan_FailsWithDuplicateFlight()
    {
        var result = FlightAssembler.Assemble([Plan("LX1612", page: 5), Plan("LX1612", page: 12)], [Briefing("LX1612", page: 97)]);

        var issue = result.FailedFlights.ShouldHaveSingleItem().Issues.ShouldHaveSingleItem();
        issue.Code.ShouldBe(IssueCodes.DuplicateFlight);
        issue.Chapter.ShouldBe(Chapters.Ofp);
        issue.PageNumber.ShouldBe(12);
    }

    [Fact]
    public void Assemble_DuplicateCrewBriefing_FailsWithDuplicateFlight()
    {
        var result = FlightAssembler.Assemble([Plan("LX1612", page: 5)], [Briefing("LX1612", page: 85), Briefing("LX1612", page: 97)]);

        var issue = result.FailedFlights.ShouldHaveSingleItem().Issues.ShouldHaveSingleItem();
        issue.Code.ShouldBe(IssueCodes.DuplicateFlight);
        issue.Chapter.ShouldBe(Chapters.CrewBriefing);
        issue.PageNumber.ShouldBe(97);
    }

    [Fact]
    public void Assemble_FlightPlanWithoutFlightNumber_FailsWithItsOwnIssues()
    {
        var missing = Issue(IssueCodes.FieldMissing, Chapters.Ofp);
        var plan = Plan("LX1612", page: 5, missing);
        plan.Builder.FlightNumber = null;

        var result = FlightAssembler.Assemble([plan], [Briefing("LX1612", page: 97)]);

        result.FailedFlights.Count.ShouldBe(2);
        result.FailedFlights[0].ShouldBe(result.FailedFlights[0] with { FlightNumber = null, Date = March19 });
        result.FailedFlights[0].Issues.ShouldBe([missing]);
        result.FailedFlights[1].Issues.ShouldHaveSingleItem().Code.ShouldBe(IssueCodes.FlightPlanMissing);
    }

    [Fact]
    public void Assemble_SamplePdf_ReturnsAllFourFlightsWithoutIssues()
    {
        var flightPlans = FlightPlanPages.Select(page => new SectionParser<OfpSectionBuilder>(Chapters.Ofp, FieldCatalog<OfpSectionBuilder>.All).Parse(sample.Page(page))).ToList();
        var crewBriefings = CrewBriefingPages.Select(page => new SectionParser<CrewBriefingSectionBuilder>(Chapters.CrewBriefing, FieldCatalog<CrewBriefingSectionBuilder>.All).Parse(sample.Page(page))).ToList();

        var result = FlightAssembler.Assemble(flightPlans, crewBriefings);

        result.FailedFlights.ShouldBeEmpty();
        result.ValidFlights.Select(flight => (flight.FlightNumber, flight.PassengersBusiness, flight.PassengersEconomy)).ShouldBe(
        [
            ("LX1612", 9, 42),
            ("LX1613", 18, 50),
            ("LX1072", 9, 31),
            ("LX1073", 1, 26),
        ]);
    }

    private static SectionParseResult<OfpSectionBuilder> Plan(string flightNumber, int page, params ParseIssue[] issues) =>
        new(page, new OfpSectionBuilder
        {
            FlightNumber = flightNumber,
            AtcCallSign = "SWR612Q",
            Date = March19,
            AircraftRegistration = "HBJVY",
            DepartureAirport = "LSZH",
            DestinationAirport = "LIMC",
            Alternate1 = "LIML",
            ScheduledDepartureUtc = new TimeOnly(8, 0),
            ScheduledArrivalUtc = new TimeOnly(8, 55),
            ZeroFuelMassKg = 34066,
            TimeToDestination = new TimeSpan(0, 48, 0),
            FuelToDestinationTons = 1.7m,
            TimeToAlternate = new TimeSpan(0, 20, 0),
            FuelToAlternateTons = 0.8m,
            MinimumFuelRequiredTons = 3.6m,
            RouteFirstNavPoint = "VEBIT",
            RouteLastNavPoint = "RIXUV",
            GainLossUsdPerTon = 0,
        }, issues);

    private static SectionParseResult<CrewBriefingSectionBuilder> Briefing(string flightNumber, int page, DateOnly? date = null, params ParseIssue[] issues) =>
        new(page, new CrewBriefingSectionBuilder
        {
            FlightNumber = flightNumber,
            Date = date ?? March19,
            PassengersBusiness = 9,
            PassengersEconomy = 42,
            DryOperatingWeightKg = 28822,
            DryOperatingIndex = 68.7m,
            Crew = [new CrewMember("Max Mustermann", "CMD")],
        }, issues);

    private static ParseIssue Issue(string code, string chapter) =>
        new(code, "message", chapter, "Field", 5, null);
}
