using FlightParser.Issues;
using FlightParser.Parsing;
using FlightParser.Parsing.Fields;
using FlightParser.Parsing.Fields.Builders;
using FlightParser.Pdf;
using FlightParser.Tests.TestSupport;

namespace FlightParser.Tests.Parsing.Fields.Ofp;

public sealed class OfpFieldsTests
{
    private readonly SectionParser<OfpSectionBuilder> _parser = new(Chapters.Ofp, FieldCatalog<OfpSectionBuilder>.All);

    [Fact]
    public void FieldCatalog_FindsAllOfpFields()
    {
        FieldCatalog<OfpSectionBuilder>.All.Select(field => field.Name).Order(StringComparer.Ordinal).ShouldBe(
        [
            "AircraftRegistration",
            "Alternate1",
            "Alternate2",
            "AtcCallSign",
            "Date",
            "DepartureAirport",
            "DestinationAirport",
            "FlightNumber",
            "GainLoss",
            "MinimumFuelRequired",
            "Route",
            "ScheduledArrival",
            "ScheduledDeparture",
            "TimeAndFuelToAlternate",
            "TimeAndFuelToDestination",
            "ZeroFuelMass",
        ]);
    }

    [Fact]
    public void Parse_Lx1612_ExtractsAllFields()
    {
        var result = Parse(5);

        result.Issues.ShouldBeEmpty();
        AssertBuilder(result.Builder, "LX1612", "SWR612Q", "HBJVY", "LSZH", "LIMC", "LIML", "08:00", "08:55", 34066, "0:48", 1.7m, "0:20", 0.8m, 3.6m, "VEBIT", "RIXUV", 0);
    }

    [Fact]
    public void Parse_Lx1613_ExtractsAllFields()
    {
        var result = Parse(12);

        result.Issues.ShouldBeEmpty();
        AssertBuilder(result.Builder, "LX1613", "SWR2TM", "HBJVY", "LIMC", "LSZH", "LFSB", "09:40", "10:45", 35337, "0:48", 1.7m, "0:39", 1.5m, 4.3m, "PEPAG", "KELIP", -40);
    }

    [Fact]
    public void Parse_Lx1072_ExtractsAllFields()
    {
        var result = Parse(19);

        result.Issues.ShouldBeEmpty();
        AssertBuilder(result.Builder, "LX1072", "SWR2ET", "HBJVN", "LSZH", "EDDF", "EDDK", "11:15", "12:20", 33107, "0:57", 1.9m, "0:47", 1.6m, 4.6m, "DEGES", "SPESA4C", -96);
    }

    [Fact]
    public void Parse_Lx1073WithWrappedRouteAndSid_ExtractsAllFields()
    {
        var result = Parse(26);

        result.Issues.ShouldBeEmpty();
        AssertBuilder(result.Builder, "LX1073", "SWR890M", "HBJVN", "EDDF", "LSZH", "LFSB", "13:45", "14:40", 31536, "0:40", 1.4m, "0:44", 1.7m, 4.2m, "ANEKI1Y", "RILAX", 60);
    }

    [Theory]
    [InlineData("FltNr: LX1612", "FlightNo: LX1612", "FlightNumber", IssueCodes.FieldMissing)]
    [InlineData("FltNr: LX1612", "FltNr: LX-1612", "FlightNumber", IssueCodes.FieldInvalidFormat)]
    [InlineData("ATC: SWR612Q", "ATC: swr612q", "AtcCallSign", IssueCodes.FieldInvalidFormat)]
    [InlineData("Date: 19MAR24", "Date: 32MAR24", "Date", IssueCodes.FieldInvalidFormat)]
    [InlineData("Reg.: HBJVY", "Reg.: _____", "AircraftRegistration", IssueCodes.FieldInvalidFormat)]
    [InlineData("From: LSZH", "From: LSZ", "DepartureAirport", IssueCodes.FieldInvalidFormat)]
    [InlineData("To: LIMC", "To: LIM", "DestinationAirport", IssueCodes.FieldInvalidFormat)]
    [InlineData("ALTN1: LIML", "ALTN1: CTOT:", "Alternate1", IssueCodes.FieldInvalidFormat)]
    [InlineData("STD: 08:00", "STD: 8h00", "ScheduledDeparture", IssueCodes.FieldInvalidFormat)]
    [InlineData("STA: 08:55", "STA:", "ScheduledArrival", IssueCodes.FieldInvalidFormat)]
    [InlineData("STA: 08:55", "", "ScheduledArrival", IssueCodes.FieldMissing)]
    [InlineData("ZFM: 34066 kg", "ZFX: 34066 kg", "ZeroFuelMass", IssueCodes.FieldMissing)]
    [InlineData("ZFM: 34066 kg", "ZFM: 34O66 kg", "ZeroFuelMass", IssueCodes.FieldInvalidFormat)]
    [InlineData("LIMC: 0:48 1.7", "XXXX: 0:48 1.7", "TimeAndFuelToDestination", IssueCodes.FieldMissing)]
    [InlineData("LIMC: 0:48 1.7", "LIMC: 0:48 x.7", "TimeAndFuelToDestination", IssueCodes.FieldInvalidFormat)]
    [InlineData("To: LIMC", "To: LIM", "TimeAndFuelToDestination", IssueCodes.FieldMissing)]
    [InlineData("LIML: 0:20 0.8", "LIML: 0:2O 0.8", "TimeAndFuelToAlternate", IssueCodes.FieldInvalidFormat)]
    [InlineData("MIN: 1:43 3.6", "MIN: 1:43 3,6", "MinimumFuelRequired", IssueCodes.FieldInvalidFormat)]
    [InlineData("Gain / Loss:", "Gain/Loss:", "GainLoss", IssueCodes.FieldMissing)]
    [InlineData("GAIN 0$/TON", "GAIN 0EUR/TON", "GainLoss", IssueCodes.FieldInvalidFormat)]
    [InlineData("To DEST:", "To DST:", "Route", IssueCodes.FieldMissing)]
    [InlineData("To DEST: VEBIT", "To DEST: vebit", "Route", IssueCodes.FieldInvalidFormat)]
    public void Parse_CorruptedField_ReportsIssueForThatField(string search, string replacement, string field, string expectedCode)
    {
        var result = Parse(5, search, replacement);

        var issue = result.Issues.Where(i => i.Field == field).ShouldHaveSingleItem();
        issue.Code.ShouldBe(expectedCode);
        issue.Chapter.ShouldBe(Chapters.Ofp);
        issue.PageNumber.ShouldBe(5);
    }

    [Fact]
    public void Parse_CorruptedField_StillExtractsAllOtherFields()
    {
        var result = Parse(5, "ZFM: 34066 kg", "ZFM: 34O66 kg");

        result.Issues.ShouldHaveSingleItem().Field.ShouldBe("ZeroFuelMass");
        result.Builder.FlightNumber.ShouldBe("LX1612");
        result.Builder.MinimumFuelRequiredTons.ShouldBe(3.6m);
    }

    [Fact]
    public void Parse_Alternate2Present_ExtractsIt()
    {
        var result = Parse(5, "ALTN2: Delay", "ALTN2: LIMF TRN TORINO/CASELLE Delay");

        result.Issues.ShouldBeEmpty();
        result.Builder.Alternate2.ShouldBe("LIMF");
    }

    [Fact]
    public void Parse_Alternate2WithInvalidCode_ReportsInvalidFormat()
    {
        var result = Parse(5, "ALTN2: Delay", "ALTN2: LIM1 TRN TORINO/CASELLE Delay");

        var issue = result.Issues.ShouldHaveSingleItem();
        issue.Field.ShouldBe("Alternate2");
        issue.Code.ShouldBe(IssueCodes.FieldInvalidFormat);
    }

    [Fact]
    public void Parse_Alternate2LabelMissing_IsNotAnIssue()
    {
        var result = Parse(5, "ALTN2:", "");

        result.Issues.ShouldBeEmpty();
        result.Builder.Alternate2.ShouldBeNull();
    }

    private SectionParseResult<OfpSectionBuilder> Parse(int pageNumber, string? search = null, string? replacement = null)
    {
        var page = TestData.PageFromFixture(pageNumber);
        if (search is not null)
        {
            page.Text.ShouldContain(search);
            page = page with { Text = page.Text.Replace(search, replacement, StringComparison.Ordinal) };
        }

        return _parser.Parse(page);
    }

    private static void AssertBuilder(
        OfpSectionBuilder builder,
        string flightNumber,
        string callSign,
        string registration,
        string departure,
        string destination,
        string alternate1,
        string std,
        string sta,
        int zeroFuelMassKg,
        string timeToDestination,
        decimal fuelToDestination,
        string timeToAlternate,
        decimal fuelToAlternate,
        decimal minimumFuel,
        string firstNavPoint,
        string lastNavPoint,
        int gainLoss)
    {
        builder.ShouldSatisfyAllConditions(
            () => builder.FlightNumber.ShouldBe(flightNumber),
            () => builder.AtcCallSign.ShouldBe(callSign),
            () => builder.Date.ShouldBe(new DateOnly(2024, 3, 19)),
            () => builder.AircraftRegistration.ShouldBe(registration),
            () => builder.DepartureAirport.ShouldBe(departure),
            () => builder.DestinationAirport.ShouldBe(destination),
            () => builder.Alternate1.ShouldBe(alternate1),
            () => builder.Alternate2.ShouldBeNull(),
            () => builder.ScheduledDepartureUtc.ShouldBe(TimeOnly.Parse(std, System.Globalization.CultureInfo.InvariantCulture)),
            () => builder.ScheduledArrivalUtc.ShouldBe(TimeOnly.Parse(sta, System.Globalization.CultureInfo.InvariantCulture)),
            () => builder.ZeroFuelMassKg.ShouldBe(zeroFuelMassKg),
            () => builder.TimeToDestination.ShouldBe(Duration(timeToDestination)),
            () => builder.FuelToDestinationTons.ShouldBe(fuelToDestination),
            () => builder.TimeToAlternate.ShouldBe(Duration(timeToAlternate)),
            () => builder.FuelToAlternateTons.ShouldBe(fuelToAlternate),
            () => builder.MinimumFuelRequiredTons.ShouldBe(minimumFuel),
            () => builder.RouteFirstNavPoint.ShouldBe(firstNavPoint),
            () => builder.RouteLastNavPoint.ShouldBe(lastNavPoint),
            () => builder.GainLossUsdPerTon.ShouldBe(gainLoss));
    }

    private static TimeSpan Duration(string value) =>
        FieldValues.TryParseDuration(value, out var duration) ? duration : throw new ArgumentException(value, nameof(value));
}
