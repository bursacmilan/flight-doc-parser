using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using FlightParser.Issues;
using FlightParser.Parsing.Fields;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Pdf;

namespace FlightParser.Tests.Parsing.Fields;

public sealed partial class FieldTests
{
    [Fact]
    public void Extract_ValueFound_AppliesValueWithoutProblem()
    {
        var builder = new TestBuilder();

        var problem = new NumberField().Extract(Page("NUM: 42"), builder);

        problem.ShouldBeNull();
        builder.Number.ShouldBe(42);
    }

    [Fact]
    public void Extract_RequiredLabelMissing_ReportsMissing()
    {
        var problem = new NumberField().Extract(Page("nothing here"), new TestBuilder());

        problem.ShouldNotBeNull();
        problem.Code.ShouldBe(IssueCodes.FieldMissing);
        problem.Message.ShouldContain("NUM:");
    }

    [Fact]
    public void Extract_OptionalLabelMissing_ReportsNothing()
    {
        var builder = new TestBuilder();

        var problem = new OptionalNumberField().Extract(Page("nothing here"), builder);

        problem.ShouldBeNull();
        builder.Number.ShouldBeNull();
    }

    [Fact]
    public void Extract_ValueWithWrongFormat_ReportsInvalidFormatWithRawText()
    {
        var problem = new NumberField().Extract(Page("NUM: 4x2"), new TestBuilder());

        problem.ShouldNotBeNull();
        problem.Code.ShouldBe(IssueCodes.FieldInvalidFormat);
        problem.RawText.ShouldBe("NUM: 4x2");
        problem.Message.ShouldContain("4x2");
    }

    private static PageContent Page(string text) => new(7, text, []);

    internal sealed class TestBuilder
    {
        public int? Number { get; set; }
    }

    internal partial class NumberField : RegexField<TestBuilder, int>
    {
        public override string Name => "Number";

        protected override string Label => "NUM:";

        protected override Regex Pattern => NumberLine();

        protected override bool TryConvert(string rawValue, [MaybeNullWhen(false)] out int value) => FieldValues.TryParseInteger(rawValue, out value);

        protected override void Apply(TestBuilder builder, int value) => builder.Number = value;

        [GeneratedRegex(@"^NUM:\s+(?<value>\S+)", RegexOptions.Multiline)]
        private static partial Regex NumberLine();
    }

    internal sealed class OptionalNumberField : NumberField
    {
        protected override bool IsRequired => false;
    }
}
