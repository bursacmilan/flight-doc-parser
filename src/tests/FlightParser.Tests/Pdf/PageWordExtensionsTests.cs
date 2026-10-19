using FlightParser.Pdf;

namespace FlightParser.Tests.Pdf;

public sealed class PageWordExtensionsTests
{
    [Fact]
    public void ToLines_WordsOnSameBaseline_AreJoinedFromLeftToRight()
    {
        PageWord[] words = [Word("08:00", 60, 500), Word("STD:", 40, 500), Word("Date:", 10, 500)];

        var lines = words.ToLines();

        lines.ShouldBe(["Date: STD: 08:00"]);
    }

    [Fact]
    public void ToLines_SlightlyShiftedBaselineWithinTolerance_StaysOnSameLine()
    {
        PageWord[] words = [Word("DOW:", 10, 500), Word("28916kg", 50, 499.2)];

        var lines = words.ToLines();

        lines.ShouldBe(["DOW: 28916kg"]);
    }

    [Fact]
    public void ToLines_LeftWordSlightlyLowerWithinTolerance_KeepsLeftToRightOrder()
    {
        PageWord[] words = [Word("Remarks:", 60, 500), Word("Scheduled", 10, 499.2)];

        var lines = words.ToLines();

        lines.ShouldBe(["Scheduled Remarks:"]);
    }

    [Fact]
    public void ToLines_DifferentBaselines_AreOrderedFromTopToBottom()
    {
        PageWord[] words = [Word("bottom", 10, 100), Word("top", 10, 700), Word("middle", 10, 400)];

        var lines = words.ToLines();

        lines.ShouldBe(["top", "middle", "bottom"]);
    }

    [Fact]
    public void ToLines_WithoutWords_ReturnsNoLines()
    {
        Array.Empty<PageWord>().ToLines().ShouldBeEmpty();
    }

    [Fact]
    public void ToText_MultipleLines_JoinsLinesWithLineBreaks()
    {
        PageWord[] words = [Word("DOW:", 10, 500), Word("28916kg", 50, 500), Word("DOI:", 10, 490), Word("68.7", 50, 490)];

        var text = words.ToText();

        text.ShouldBe("DOW: 28916kg\nDOI: 68.7");
    }

    private static PageWord Word(string text, double left, double bottom) =>
        new(text, bottom + 6, left + (text.Length * 4), bottom, left);
}
