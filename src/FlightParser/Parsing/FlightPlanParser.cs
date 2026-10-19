using FlightParser.Issues;
using FlightParser.Parsing.Assembly;
using FlightParser.Parsing.Fields;
using FlightParser.Parsing.Fields.Builders;
using FlightParser.Parsing.PageClassifiers;
using FlightParser.Pdf;
using FlightParser.Results;
using Microsoft.Extensions.Logging;

namespace FlightParser.Parsing;

internal sealed partial class FlightPlanParser(ILogger<FlightPlanParser> logger, IPdfTextExtractor extractor) : IFlightPlanParser
{
    private readonly OfpPageClassifier _ofpClassifier = new();
    private readonly CrewBriefingPageClassifier _crewBriefingClassifier = new();
    private readonly SectionParser<OfpSectionBuilder> _ofpParser = new(Chapters.Ofp, FieldCatalog<OfpSectionBuilder>.All);
    private readonly SectionParser<CrewBriefingSectionBuilder> _crewBriefingParser = new(Chapters.CrewBriefing, FieldCatalog<CrewBriefingSectionBuilder>.All);

    public async Task<FlightPlanParseResult> ParseAsync(Stream pdf, CancellationToken cancellationToken = default)
    {
        var readResult = await extractor.ExtractAsync(pdf, cancellationToken).ConfigureAwait(false);
        if (readResult is PdfReadResult.Failure failure)
        {
            LogRejected(failure.Issue.Code);
            return new FlightPlanParseResult.Rejected([failure.Issue]);
        }

        var pages = ((PdfReadResult.Success)readResult).Pages;
        LogPagesRead(pages.Count);

        var flightPlans = ParseSections(pages, _ofpClassifier, _ofpParser, cancellationToken);
        if (flightPlans.Count == 0)
        {
            return Reject(IssueCodes.NoFlightPlanFound, Chapters.Ofp, "The document contains no Operational Flight Plan.");
        }

        var crewBriefings = ParseSections(pages, _crewBriefingClassifier, _crewBriefingParser, cancellationToken);
        if (crewBriefings.Count == 0)
        {
            return Reject(IssueCodes.NoCrewBriefingFound, Chapters.CrewBriefing, "The document contains no Crew Briefing.");
        }

        LogSectionsFound(flightPlans.Count, crewBriefings.Count);

        var result = FlightAssembler.Assemble(flightPlans, crewBriefings);
        LogParsed(result.ValidFlights.Count, result.FailedFlights.Count);

        return result;
    }

    private FlightPlanParseResult.Rejected Reject(string code, string chapter, string message)
    {
        LogRejected(code);
        return new FlightPlanParseResult.Rejected(
            [new ParseIssue(code, message, chapter, Field: null, PageNumber: null, RawText: null)]);
    }

    private static List<SectionParseResult<TBuilder>> ParseSections<TBuilder>(
        IReadOnlyList<PageContent> pages,
        IPageClassifier classifier,
        SectionParser<TBuilder> parser,
        CancellationToken cancellationToken)
        where TBuilder : new()
    {
        var sections = new List<SectionParseResult<TBuilder>>();
        foreach (var page in pages.Where(classifier.IsMatch))
        {
            cancellationToken.ThrowIfCancellationRequested();
            sections.Add(parser.Parse(page));
        }

        return sections;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Read {PageCount} pages from the PDF.")]
    private partial void LogPagesRead(int pageCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Found {FlightPlanCount} Operational Flight Plans and {CrewBriefingCount} Crew Briefings.")]
    private partial void LogSectionsFound(int flightPlanCount, int crewBriefingCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Parsed {ValidCount} valid and {FailedCount} failed flights.")]
    private partial void LogParsed(int validCount, int failedCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Rejected the document with issue {IssueCode}.")]
    private partial void LogRejected(string issueCode);
}
