using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Published;
using PUSpecSheet.DataAccess.Published.Sheets;
using PUSpecSheet.DataAccess.Published.Versions;

namespace PUSpecSheet.DataAccess.Tests.Published.Sheets;

/// <summary>
/// What a caller receives from the rows the queries return: only what was on the sheet at the version, in
/// display order, with only the cells that hold a value.
/// </summary>
public sealed class PublishedSheetAssemblerTests
{
    private static readonly ResolvedSheetVersion _version = new ResolvedSheetVersion
    {
        SheetId = 1,
        SheetPublicId = Id(1),
        VersionNumber = 3,
        PublishedAtUtc = new DateTime(2026, 9, 30, 14, 0, 0, DateTimeKind.Utc),
    };

    // Table 1 holds a header section 10 and a section 20 with a sub-section 21. Table 2 was removed, along
    // with its section 30. Section 22 was removed from table 1. Table 1 has column blocks 5 then 6.
    private static readonly PublishedStructure _structure = new(
        [
            new PublishedTableRecord { Id = 1, PublicId = Id(100), DisplayOrder = 0, IsDeleted = false, Title = "Torques" },
            new PublishedTableRecord { Id = 2, PublicId = Id(200), DisplayOrder = 1, IsDeleted = true, Title = "Removed" },
        ],
        [
            new PublishedSectionRecord
            {
                Id = 20,
                PublicId = Id(120),
                TableId = 1,
                DisplayOrder = 1,
                IsDeleted = false,
                Name = "Bolts",
            },
            new PublishedSectionRecord
            {
                Id = 10,
                PublicId = Id(110),
                TableId = 1,
                DisplayOrder = 0,
                IsDeleted = false,
                Name = "Header",
            },
            new PublishedSectionRecord
            {
                Id = 21,
                PublicId = Id(121),
                TableId = 1,
                ParentSectionId = 20,
                DisplayOrder = 0,
                IsDeleted = false,
                Name = "Studs",
            },
            new PublishedSectionRecord
            {
                Id = 22,
                PublicId = Id(122),
                TableId = 1,
                DisplayOrder = 2,
                IsDeleted = true,
                Name = "Gone",
            },
            new PublishedSectionRecord
            {
                Id = 30,
                PublicId = Id(130),
                TableId = 2,
                DisplayOrder = 0,
                IsDeleted = false,
                Name = "In removed table",
            },
        ],
        [
            new PublishedColumnBlockRecord
            {
                Id = 6,
                PublicId = Id(160),
                TableId = 1,
                DisplayOrder = 1,
                IsDeleted = false,
                Name = "Second",
            },
            new PublishedColumnBlockRecord { Id = 5, PublicId = Id(150), TableId = 1, DisplayOrder = 0, IsDeleted = false, Name = "First" },
        ]);

    private static readonly PublishedCellRecord[] _cells =
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

    private static readonly Dictionary<int, object> _values = new()
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
        var sheet = PublishedSheetAssembler.Assemble(_version, Everything(), _structure, _cells, _values);

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
        var sheet = PublishedSheetAssembler.Assemble(_version, Everything(), _structure, _cells, _values);

        var row = sheet.Tables![0].Sections[1].Rows[0];

        Assert.Equal([CellId(1), CellId(5), CellId(3)], row.Cells.Select(cell => cell.Id));
        Assert.Equal(["M8", 9m, 12.5m], row.Cells.Select(cell => cell.Value));
        Assert.Equal([null, Id(150), Id(160)], row.Cells.Select(cell => cell.Column));
    }

    [Fact]
    public void Labels_AreLeftOutUnlessAskedFor()
    {
        var plain = PublishedSheetAssembler.Assemble(_version, Everything(), _structure, _cells, _values);
        var labelled = PublishedSheetAssembler.Assemble(
            _version,
            PublishedSheetSelection.Parse(null, null, null, null, PublishedSheetShape.Tree, "labels"),
            _structure,
            _cells,
            _values);

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

        var sheet = PublishedSheetAssembler.Assemble(_version, selection, _structure, _cells, _values);

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
        var found = _cells.Where(cell => cell.CellId is 6 or 7).ToList();

        var sheet = PublishedSheetAssembler.Assemble(_version, selection, _structure, found, _values);

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

        var scope = _structure.ScopeFor(selection);

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
        return new PublishedCellRecord
        {
            RowRevisionId = row * 10,
            RowId = row,
            RowPublicId = RowId(row),
            SectionId = section,
            RowOrder = rowOrder,
            CellId = cell,
            CellPublicId = CellId(cell),
            ColumnBlockId = block,
            Column = column,
            Caption = $"Caption {cell}",
        };
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
