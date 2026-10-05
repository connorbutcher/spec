using PUSpecSheet.Application.Published;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Tests.Published;

/// <summary>
/// What a caller receives from the rows the queries return: only what was on the sheet at the version, in
/// display order, with only the cells that hold a value.
/// </summary>
public sealed class PublishedSheetAssemblerTests
{
    private static readonly ResolvedSheetVersion Version = new(1, Id(1), 3, new DateTime(2026, 9, 30, 14, 0, 0, DateTimeKind.Utc));

    // Table 1 holds a header section 10 and a section 20 with a sub-section 21. Table 2 was removed, along
    // with its section 30. Section 22 was removed from table 1. Table 1 has column blocks 5 then 6.
    private static readonly PublishedStructure Structure = new(
        [
            new PublishedTableRecord(1, Id(100), 0, false, "Torques"),
            new PublishedTableRecord(2, Id(200), 1, true, "Removed"),
        ],
        [
            new PublishedSectionRecord(20, Id(120), 1, null, 1, false, "Bolts"),
            new PublishedSectionRecord(10, Id(110), 1, null, 0, false, "Header"),
            new PublishedSectionRecord(21, Id(121), 1, 20, 0, false, "Studs"),
            new PublishedSectionRecord(22, Id(122), 1, null, 2, true, "Gone"),
            new PublishedSectionRecord(30, Id(130), 2, null, 0, false, "In removed table"),
        ],
        [
            new PublishedColumnBlockRecord(6, Id(160), 1, 1, false, "Second"),
            new PublishedColumnBlockRecord(5, Id(150), 1, 0, false, "First"),
        ]);

    private static readonly PublishedCellRecord[] Cells =
    [
        Cell(row: 2, section: 20, rowOrder: 1, cell: 4, column: 1),
        Cell(row: 1, section: 20, rowOrder: 0, cell: 3, column: 1, block: 6),
        Cell(row: 1, section: 20, rowOrder: 0, cell: 2, column: 2),
        Cell(row: 1, section: 20, rowOrder: 0, cell: 1, column: 1),
        Cell(row: 1, section: 20, rowOrder: 0, cell: 5, column: 1, block: 5),
        Cell(row: 3, section: 21, rowOrder: 0, cell: 6, column: 1),
        Cell(row: 4, section: 22, rowOrder: 0, cell: 7, column: 1),
        Cell(row: 5, section: 30, rowOrder: 0, cell: 8, column: 1),
    ];

    private static readonly Dictionary<int, object> Values = new()
    {
        [1] = "M8",
        [3] = 12.5m,
        [4] = true,
        [5] = 9m,
        [6] = new DateOnly(2026, 1, 2),
        [7] = "in a removed section",
        [8] = "in a removed table",
    };

    [Fact]
    public void WholeSheet_ShowsOnlyWhatWasOnTheSheetInDisplayOrder()
    {
        var sheet = PublishedSheetAssembler.Assemble(Version, Everything(), Structure, Cells, Values);

        Assert.Equal(3, sheet.Version);
        Assert.Null(sheet.Cells);
        Assert.Empty(sheet.Missing);
        var table = Assert.Single(sheet.Tables!);
        Assert.Equal(Id(100), table.Id);
        Assert.Equal([Id(110), Id(120)], table.Sections.Select(section => section.Id));
        Assert.Equal([Id(150), Id(160)], table.Columns!.Select(block => block.Id));

        var bolts = table.Sections[1];
        Assert.Equal([RowId(1), RowId(2)], bolts.Rows.Select(row => row.Id));
        Assert.Equal(Id(121), Assert.Single(bolts.Sections).Id);
    }

    [Fact]
    public void ARowsCells_AreItsOwnThenEachColumnBlocksAndOnlyThoseWithAValue()
    {
        var sheet = PublishedSheetAssembler.Assemble(Version, Everything(), Structure, Cells, Values);

        var row = sheet.Tables![0].Sections[1].Rows[0];

        Assert.Equal([CellId(1), CellId(5), CellId(3)], row.Cells.Select(cell => cell.Id));
        Assert.Equal(["M8", 9m, 12.5m], row.Cells.Select(cell => cell.Value));
        Assert.Equal([null, Id(150), Id(160)], row.Cells.Select(cell => cell.Column));
    }

    [Fact]
    public void Labels_AreLeftOutUnlessAskedFor()
    {
        var plain = PublishedSheetAssembler.Assemble(Version, Everything(), Structure, Cells, Values);
        var labelled = PublishedSheetAssembler.Assemble(
            Version,
            PublishedSheetSelection.Parse(null, null, null, null, PublishedSheetShape.Tree, "labels"),
            Structure,
            Cells,
            Values);

        Assert.Null(plain.Tables![0].Title);
        Assert.Null(plain.Tables[0].Sections[0].Name);
        Assert.Equal("Torques", labelled.Tables![0].Title);
        Assert.Equal("Header", labelled.Tables[0].Sections[0].Name);
        Assert.Equal("First", labelled.Tables[0].Columns![0].Name);
    }

    [Fact]
    public void Flat_MapsEachCellWithAValueAndSkipsRemovedParts()
    {
        var selection = PublishedSheetSelection.Parse(null, null, null, null, PublishedSheetShape.Flat, null);

        var sheet = PublishedSheetAssembler.Assemble(Version, selection, Structure, Cells, Values);

        Assert.Null(sheet.Tables);
        Assert.Equal(
            new[] { CellId(1), CellId(3), CellId(4), CellId(5), CellId(6) }.Order(),
            sheet.Cells!.Keys.Order());
        Assert.Equal(12.5m, sheet.Cells[CellId(3)]);
    }

    [Fact]
    public void ASelection_ShowsOnlyTheBranchesThatLeadToItAndListsWhatIsMissing()
    {
        var unknown = Guid.NewGuid();
        var selection = PublishedSheetSelection.Parse(
            null,
            Id(122).ToString(),
            null,
            $"{CellId(6)},{CellId(7)},{unknown}",
            PublishedSheetShape.Tree,
            null);
        var found = Cells.Where(cell => cell.CellId is 6 or 7).ToList();

        var sheet = PublishedSheetAssembler.Assemble(Version, selection, Structure, found, Values);

        var bolts = Assert.Single(Assert.Single(sheet.Tables!).Sections);
        Assert.Equal(Id(120), bolts.Id);
        Assert.Empty(bolts.Rows);
        Assert.Equal(CellId(6), Assert.Single(Assert.Single(Assert.Single(bolts.Sections).Rows).Cells).Id);
        Assert.Equal(new[] { Id(122), CellId(7), unknown }.Order(), sheet.Missing.Order());
    }

    [Fact]
    public void ASectionSelection_ReachesItsSubSections()
    {
        var selection = PublishedSheetSelection.Parse(Id(200).ToString(), Id(120).ToString(), null, null, PublishedSheetShape.Tree, null);

        var scope = Structure.ScopeFor(selection);

        Assert.Empty(scope.TableIds);
        Assert.Equal([20, 21], scope.SectionIds.Order());
        Assert.True(scope.HasAny);
    }

    private static PublishedSheetSelection Everything()
    {
        return PublishedSheetSelection.Parse(null, null, null, null, PublishedSheetShape.Tree, null);
    }

    private static PublishedCellRecord Cell(int row, int section, int rowOrder, int cell, int column, int? block = null)
    {
        return new PublishedCellRecord(row * 10, row, RowId(row), section, rowOrder, cell, CellId(cell), block, column, $"Caption {cell}");
    }

    private static Guid Id(int value)
    {
        return new Guid(value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }

    private static Guid RowId(int row)
    {
        return Id(1000 + row);
    }

    private static Guid CellId(int cell)
    {
        return Id(2000 + cell);
    }
}
