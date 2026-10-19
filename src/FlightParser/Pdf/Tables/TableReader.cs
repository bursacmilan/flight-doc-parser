namespace FlightParser.Pdf.Tables;

internal static class TableReader
{
    private const double Tolerance = 1.5;
    private const double MinimumCellSize = 3.0;
    private const double MinimumRowCoverage = 0.5;

    public static IReadOnlyList<PageTable> ReadTables(this PageContent page) =>
    [
        .. GroupConnectedLines(page.Lines)
            .Select(group => BuildTable(group, page.Words))
            .OfType<PageTable>()
    ];

    private static IEnumerable<List<LineSegment>> GroupConnectedLines(IReadOnlyList<LineSegment> lines)
    {
        var parent = Enumerable.Range(0, lines.Count).ToArray();

        for (var a = 0; a < lines.Count; a++)
        {
            for (var b = a + 1; b < lines.Count; b++)
            {
                if (Intersect(lines[a], lines[b]))
                {
                    parent[Root(a)] = Root(b);
                }
            }
        }

        return Enumerable.Range(0, lines.Count)
            .GroupBy(Root)
            .Select(group => group.Select(index => lines[index]).ToList())
            .OrderByDescending(group => group.Max(line => line.Orientation == LineOrientation.Horizontal ? line.Position : line.End));

        int Root(int index)
        {
            while (parent[index] != index)
            {
                parent[index] = parent[parent[index]];
                index = parent[index];
            }

            return index;
        }
    }

    private static bool Intersect(LineSegment a, LineSegment b)
    {
        if (a.Orientation == b.Orientation)
        {
            return false;
        }

        var (horizontal, vertical) = a.Orientation == LineOrientation.Horizontal ? (a, b) : (b, a);

        return vertical.Position >= horizontal.Start - Tolerance
            && vertical.Position <= horizontal.End + Tolerance
            && horizontal.Position >= vertical.Start - Tolerance
            && horizontal.Position <= vertical.End + Tolerance;
    }

    private static PageTable? BuildTable(List<LineSegment> lines, IReadOnlyList<PageWord> words)
    {
        var verticals = lines.Where(line => line.Orientation == LineOrientation.Vertical).ToList();
        var rowEdges = Merge(lines.Where(line => line.Orientation == LineOrientation.Horizontal).Select(line => line.Position))
            .OrderByDescending(y => y)
            .ToList();

        var rows = new List<TableRow>();
        for (var index = 0; index < rowEdges.Count - 1; index++)
        {
            var top = rowEdges[index];
            var bottom = rowEdges[index + 1];
            if (top - bottom < MinimumCellSize)
            {
                continue;
            }

            var columnEdges = Merge(verticals.Where(line => Overlap(line, bottom, top) >= (top - bottom) * MinimumRowCoverage).Select(line => line.Position))
                .Order()
                .ToList();

            var cells = columnEdges.Zip(columnEdges.Skip(1))
                .Where(edge => edge.Second - edge.First >= MinimumCellSize)
                .Select(edge => CellText(words, edge.First, edge.Second, bottom, top))
                .ToList();

            if (cells.Count > 0)
            {
                rows.Add(new TableRow(cells));
            }
        }

        return rows.Count > 0 ? new PageTable(rows) : null;
    }

    private static double Overlap(LineSegment vertical, double bottom, double top) =>
        Math.Min(vertical.End, top) - Math.Max(vertical.Start, bottom);

    private static List<double> Merge(IEnumerable<double> positions)
    {
        var merged = new List<double>();
        foreach (var position in positions.Order())
        {
            if (merged.Count == 0 || position - merged[^1] > Tolerance * 2)
            {
                merged.Add(position);
            }
        }

        return merged;
    }

    private static string CellText(IReadOnlyList<PageWord> words, double left, double right, double bottom, double top) =>
        string.Join(' ', words
            .Where(word => IsInside((word.Left + word.Right) / 2, (word.Bottom + word.Top) / 2, left, right, bottom, top))
            .ToLines());

    private static bool IsInside(double x, double y, double left, double right, double bottom, double top) =>
        x > left && x < right && y > bottom && y < top;
}
