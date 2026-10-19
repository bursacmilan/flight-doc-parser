namespace FlightParser.Pdf;

internal static class PageWordExtensions
{
    // Tolerance is the half of the height of the word, but at least 1.0.
    private const double BaselineToleranceFactor = 0.5;
    private const double MinimumBaselineTolerance = 1.0;

    extension(IEnumerable<PageWord> words)
    {
        public string ToText() => string.Join('\n', words.ToLines());

        public IReadOnlyList<string> ToLines()
        {
            var lines = new List<string>();
            var wordsOnCurrentLine = new List<PageWord>();
            
            var lineBaseline = 0.0;
            var tolerance = 0.0;

            foreach (var word in words.OrderByDescending(w => w.Bottom).ThenBy(w => w.Left))
            {
                if (wordsOnCurrentLine.Count > 0 && lineBaseline - word.Bottom > tolerance)
                {
                    lines.Add(Join(wordsOnCurrentLine));
                    wordsOnCurrentLine.Clear();
                }

                if (wordsOnCurrentLine.Count == 0)
                {
                    lineBaseline = word.Bottom;
                    tolerance = Math.Max(word.Height * BaselineToleranceFactor, MinimumBaselineTolerance);
                }

                wordsOnCurrentLine.Add(word);
            }

            if (wordsOnCurrentLine.Count > 0)
            {
                lines.Add(Join(wordsOnCurrentLine));
            }

            return lines;
        }
    }

    private static string Join(IEnumerable<PageWord> line) =>
        string.Join(' ', line.OrderBy(w => w.Left).Select(w => w.Text));
}
