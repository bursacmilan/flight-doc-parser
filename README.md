# Flight Doc Parser

.NET 10 library that extracts flight data from flight planning PDFs (Operational Flight Plan + Crew Briefing), plus a demo console app.

## Usage

```csharp
using FlightParser;
using FlightParser.DependencyInjection;
using FlightParser.Results;
using Microsoft.Extensions.DependencyInjection;

// With dependency injection (logging is optional)
await using var services = new ServiceCollection()
    .AddLogging()
    .AddFlightPlanParser()
    .BuildServiceProvider();
var parser = services.GetRequiredService<IFlightPlanParser>();

// Without dependency injection
// var parser = FlightPlanParserFactory.Create();

await using var pdf = File.OpenRead("flight-plans.pdf");
var result = await parser.ParseAsync(pdf);

switch (result)
{
    case FlightPlanParseResult.Parsed parsed:
        foreach (var flight in parsed.ValidFlights)
        {
            Console.WriteLine($"{flight.FlightNumber} {flight.Date}: {flight.DepartureAirport}-{flight.DestinationAirport}, {flight.Crew.Count} crew");
        }

        foreach (var failed in parsed.FailedFlights)
        {
            foreach (var issue in failed.Issues)
            {
                Console.WriteLine($"{failed.FlightNumber}: {issue.Code} {issue.Message} (page {issue.PageNumber})");
            }
        }

        break;

    case FlightPlanParseResult.Rejected rejected:
        foreach (var issue in rejected.Issues)
        {
            Console.WriteLine($"Document rejected: {issue.Code} {issue.Message}");
        }

        break;
}
```

Console app (run from `src/`):

```
dotnet run --project FlightParser.ConsoleApp -- <path-to-pdf>    # path is prompted if omitted
```

- stdout: valid flights as JSON
- stderr: issues and log
- exit codes: `0` all valid, `1` some flights failed, `2` document rejected, `3` invalid usage

## How it works

- **PDF reading:** PdfPig provides words with positions and the page graphics.
  - Own line builder: words on the same baseline → one text line, so both OFP columns end up on the same line.
  - Graphics → horizontal/vertical line segments (for tables).
- **Page classification:** only page 1 of each OFP and each Crew Briefing is used, everything else is ignored.
- **Field parsers:** one class per field.
  - Regex finds the **label** and captures the **value** loosely (e.g. `ZFM:\s+(?<value>\S+)`).
  - Strict conversion afterwards: label missing → `FIELD_MISSING`, value broken → `FIELD_INVALID_FORMAT` (with raw text and page).
  - Fuel rows have no fixed label but are named after the airport (`LIMC: 0:48 1.7`). The field first reads the ICAO code after `To:` / `ALTN1:`, then the row starting with that code.
  - Route: first and last element after `To DEST:`, speed/level suffix stripped (`GERSA/N0401F230` → `GERSA`).
  - New field = new class; fields are discovered by reflection.
- **Table reader:** line segments → tables → rows → cells, words assigned by position. Used for the crew list (wrapped names).
- **Assembler:** OFP and Crew Briefing matched by flight number + date. Any error → flight discarded (`FailedFlight`) with all issues.
- **Result pattern:** `Parsed` / `Rejected`. No exceptions for expected problems; only `OperationCanceledException`.

## Architecture

```
src/
├─ FlightParser.slnx, Directory.Build.props, Directory.Packages.props, global.json, nuget.config
├─ FlightParser/                    library
│  ├─ (root), DependencyInjection/  public entry points: IFlightPlanParser, factory, AddFlightPlanParser()
│  ├─ Results/, Models/, Issues/    public result types
│  ├─ Pdf/                          internal: PdfPig adapter, line builder, Tables/ (table reader)
│  └─ Parsing/                      internal: parser, page classifiers, fields (Ofp/, Crew/, Abstractions/ …), assembler
├─ FlightParser.ConsoleApp/         demo
└─ tests/FlightParser.Tests/        unit tests (xUnit)
```

- Only the types under the public folders are public; everything else is `internal`.
- No PdfPig types in the public API.

## NuGet packages

| Package | Used for |
|---|---|
| PdfPig 0.1.16 | PDF text, word positions, graphics |
| Microsoft.Extensions.DependencyInjection.Abstractions / Logging.Abstractions | DI registration, logging in the library |
| Microsoft.Extensions.DependencyInjection / Logging.Console | console app |
| xunit.v3, Shouldly, Microsoft.NET.Test.Sdk | tests |

## Tests

```
cd src
dotnet test
```

- Many tests run against the sample PDF of the assignment. It has to be placed at `spec/Task 1 - Extract Data from a PDF File - Sample File.pdf`.
- `spec/` is **deliberately not committed**: it contains the assignment and real crew names. Without it, the test project does not build (`MSB3030`, the PDF is copied to the test output).

## Assumptions / open points

- Route first/last nav point: first and last element of the route. Departure/arrival procedures (SID/STAR, e.g. `ANEKI1Y`) count as nav points (LX1073: `ANEKI1Y`–`RILAX`).
- All fields are required except Alternate 2.
- STD/STA are returned as UTC `TimeOnly`.
- Only the operating crew is returned (no observers/jumpseat).
- DOI `0` is accepted as a normal value.

## Built with AI

Implemented together with an AI assistant (Claude):

- **Structure, architecture and design decisions:** me.
- **Mostly AI-generated boilerplate:** regex fields, field parsers and tests.
