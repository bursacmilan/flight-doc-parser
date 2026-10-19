using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FlightParser.Pdf;

namespace FlightParser.Tests.TestSupport;

internal static partial class TestData
{
    public static readonly IReadOnlyList<int> FixturePages = [1, 5, 6, 12, 19, 26, 85, 86, 89, 93, 97];

    public static string SamplePdfPath => Path.Combine(AppContext.BaseDirectory, "TestData", "SampleFile.pdf");

    public static FileStream OpenSamplePdf() => File.OpenRead(SamplePdfPath);

    public static PageContent PageFromFixture(int pageNumber) => new(pageNumber, ReadPageFixture(pageNumber), []);

    public static string ReadPageFixture(int pageNumber) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "Pages", FixtureFileName(pageNumber)));

    public static void WritePageFixture(int pageNumber, string text) =>
        File.WriteAllText(Path.Combine(SourcePagesDirectory(), FixtureFileName(pageNumber)), text);

    public static string WithoutCrewNames(string text) => CrewRows().Replace(text, "${header}");

    public static string WithCrewNamesFrom(string text, string fixture) =>
        CrewRows().Match(fixture) is { Success: true } stored
            ? CrewRows().Replace(text, match => match.Groups["header"].Value + stored.Groups["rows"].Value)
            : text;

    private static string FixtureFileName(int pageNumber) => $"page-{pageNumber:000}.txt";

    private static string SourcePagesDirectory([CallerFilePath] string callerFile = "") =>
        Path.Combine(Path.GetDirectoryName(callerFile)!, "..", "TestData", "Pages");

    [GeneratedRegex(@"(?<header>^Func 3LC Name[^\n]*\n[^\n]*\n)(?<rows>(?s:.*?))(?=^X: Operating crewmember)", RegexOptions.Multiline)]
    private static partial Regex CrewRows();
}
