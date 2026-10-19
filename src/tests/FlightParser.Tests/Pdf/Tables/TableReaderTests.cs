using FlightParser.Pdf;
using FlightParser.Pdf.Tables;

namespace FlightParser.Tests.Pdf.Tables;

public sealed class TableReaderTests(SamplePdfFixture sample) : IClassFixture<SamplePdfFixture>
{
    private static readonly string[] CrewHeader = ["Func", "3LC", "Name"];
    private static readonly string[] ObserverHeader = ["Function", "Name"];
    private static readonly string[] CrewFunctions = ["CMD", "COP", "CAB", "CAB", "SEN"];
    private static readonly string[] CrewRoles = ["Commander", "Copilot", "Cabin Attendant", "Cabin Attendant", "Senior Cabin Attendant"];

    [Fact]
    public void ReadTables_CrewBriefingLx1613_ReadsCrewTableWithWrappedNames()
    {
        var crew = CrewTable(sample.Page(89));

        crew[0].Cells.ShouldBe(["Func", "3LC", "Name", "LX1613 LIMC-LSZH"]);
        crew.Skip(1).Select(row => row.Cells[0]).ShouldBe(CrewFunctions);
        crew.Skip(1).ShouldAllBe(row => row.Cells[1].Length == 3 && row.Cells[1].All(char.IsAsciiLetterUpper));
        crew.Skip(1).Select(row => row.Cells[2].Split(' ').Length).ShouldBe([2, 2, 3, 2, 3]);
        crew.Skip(1).Select(row => row.Cells[3]).ShouldBe(CrewRoles);
    }

    [Theory]
    [InlineData(85, 2)]
    [InlineData(89, 3)]
    [InlineData(93, 2)]
    [InlineData(97, 3)]
    public void ReadTables_AllCrewBriefings_ReadFiveCrewRowsWithFourColumns(int pageNumber, int seniorCabinAttendantNameWords)
    {
        var crew = CrewTable(sample.Page(pageNumber));

        crew.Count.ShouldBe(6);
        crew.ShouldAllBe(row => row.Cells.Count == 4);
        crew[^1].Cells[0].ShouldBe("SEN");
        crew[^1].Cells[2].Split(' ').Length.ShouldBe(seniorCabinAttendantNameWords);
    }

    [Fact]
    public void ReadTables_CrewBriefing_ReadsObserverTableSeparately()
    {
        var tables = sample.Page(85).ReadTables();

        tables.ShouldContain(table => table.Rows[0].Cells.SequenceEqual(ObserverHeader));
    }

    [Fact]
    public void ReadTables_SimpleGrid_AssignsWordsToCells()
    {
        var page = new PageContent(1, "", [Word("A1", 15, 85), Word("B1", 55, 85), Word("A2", 15, 65), Word("B2", 55, 65)])
        {
            Lines = Grid(xs: [10, 50, 90], ys: [100, 80, 60]),
        };

        var table = page.ReadTables().ShouldHaveSingleItem();

        table.Rows.Select(row => row.Cells).ShouldBe([["A1", "B1"], ["A2", "B2"]]);
    }

    [Fact]
    public void ReadTables_TitleRowSpanningAllColumns_HasSingleCell()
    {
        List<LineSegment> lines =
        [
            Horizontal(120, 10, 90), Horizontal(100, 10, 90), Horizontal(80, 10, 90),
            Vertical(10, 80, 120), Vertical(90, 80, 120), Vertical(50, 80, 100),
        ];
        var page = new PageContent(1, "", [Word("Title", 30, 105), Word("A", 15, 85), Word("B", 55, 85)]) { Lines = lines };

        var table = page.ReadTables().ShouldHaveSingleItem();

        table.Rows.Select(row => row.Cells).ShouldBe([["Title"], ["A", "B"]]);
    }

    [Fact]
    public void ReadTables_WrappedCellContent_IsJoinedFromTopToBottom()
    {
        var page = new PageContent(1, "", [Word("Muster-Beispiel", 15, 65), Word("Erika", 15, 85), Word("Johanna", 40, 85)])
        {
            Lines = Grid(xs: [10, 90], ys: [100, 60]),
        };

        page.ReadTables().ShouldHaveSingleItem().Rows.ShouldHaveSingleItem().Cells.ShouldBe(["Erika Johanna Muster-Beispiel"]);
    }

    [Fact]
    public void ReadTables_TwoSeparateGrids_ReturnsTwoTablesFromTopToBottom()
    {
        var page = new PageContent(1, "", [Word("lower", 15, 25), Word("upper", 15, 185)])
        {
            Lines = [.. Grid(xs: [10, 90], ys: [40, 10]), .. Grid(xs: [10, 90], ys: [200, 170])],
        };

        page.ReadTables().Select(table => table.Rows[0].Cells[0]).ShouldBe(["upper", "lower"]);
    }

    [Fact]
    public void ReadTables_WithoutLines_ReturnsNoTables()
    {
        new PageContent(1, "text", [Word("text", 10, 10)]).ReadTables().ShouldBeEmpty();
    }

    private static List<TableRow> CrewTable(PageContent page) =>
        page.ReadTables().Single(table => table.Rows[0].Cells.Take(3).SequenceEqual(CrewHeader)).Rows.ToList();

    private static List<LineSegment> Grid(double[] xs, double[] ys) =>
    [
        .. ys.Select(y => Horizontal(y, xs[0], xs[^1])),
        .. xs.Select(x => Vertical(x, ys[^1], ys[0])),
    ];

    private static LineSegment Horizontal(double y, double start, double end) => new(LineOrientation.Horizontal, y, start, end);

    private static LineSegment Vertical(double x, double start, double end) => new(LineOrientation.Vertical, x, start, end);

    private static PageWord Word(string text, double left, double bottom) =>
        new(text, bottom + 6, left + (text.Length * 4), bottom, left);
}
