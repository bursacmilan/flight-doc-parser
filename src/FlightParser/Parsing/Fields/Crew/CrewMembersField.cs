using System.Text.RegularExpressions;
using FlightParser.Models;
using FlightParser.Parsing.Fields.Abstractions;
using FlightParser.Parsing.Fields.Builders;
using FlightParser.Pdf;
using FlightParser.Pdf.Tables;

namespace FlightParser.Parsing.Fields.Crew;

internal sealed partial class CrewMembersField : Field<CrewBriefingSectionBuilder, IReadOnlyList<CrewMember>>
{
    private const int FunctionColumn = 0;
    private const int NameColumn = 2;
    private static readonly string[] Header = ["Func", "3LC", "Name"];

    public override string Name => "Crew";

    protected override FieldResult<IReadOnlyList<CrewMember>> ExtractValue(PageContent page)
    {
        var table = page.ReadTables().FirstOrDefault(IsCrewTable);
        if (table is null)
        {
            return new FieldResult<IReadOnlyList<CrewMember>>.NotFound("Crew table with the header 'Func | 3LC | Name' not found.");
        }

        var members = new List<CrewMember>();
        foreach (var row in table.Rows.Skip(1).Where(row => row.Cells.Count > NameColumn && FunctionCode().IsMatch(row.Cells[FunctionColumn])))
        {
            if (string.IsNullOrWhiteSpace(row.Cells[NameColumn]))
            {
                return new FieldResult<IReadOnlyList<CrewMember>>.Invalid(string.Join(" | ", row.Cells), "A crew row has a function but no name.");
            }

            members.Add(new CrewMember(row.Cells[NameColumn], row.Cells[FunctionColumn]));
        }

        return members.Count > 0
            ? new FieldResult<IReadOnlyList<CrewMember>>.Found(members)
            : new FieldResult<IReadOnlyList<CrewMember>>.NotFound("The crew table contains no crew members.");
    }

    protected override void Apply(CrewBriefingSectionBuilder builder, IReadOnlyList<CrewMember> value) => builder.Crew = value;

    private static bool IsCrewTable(PageTable table) =>
        table.Rows.Count > 0 && table.Rows[0].Cells.Take(Header.Length).SequenceEqual(Header);

    [GeneratedRegex(@"^[A-Z]{2,4}$")]
    private static partial Regex FunctionCode();
}
