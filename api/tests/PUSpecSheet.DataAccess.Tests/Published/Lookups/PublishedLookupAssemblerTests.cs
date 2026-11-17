using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.DataAccess.Published;
using PUSpecSheet.DataAccess.Published.Lookups;
using PUSpecSheet.DataAccess.Published.Versions;

namespace PUSpecSheet.DataAccess.Tests.Published.Lookups;

/// <summary>
/// What a caller receives for a cell a lookup found, as rows by identifier: the column of a cell in a column block, or the rows
/// of a cell among a row's own cells, on a horizontal table and a vertical one.
/// </summary>
public sealed class PublishedLookupAssemblerTests
{
    private const string PartNumberKey = "partNumber";

    private static readonly ResolvedSheetVersion _version = new ResolvedSheetVersion
    {
        SheetId = 1,
        SheetPublicId = Id(1),
        VersionNumber = 2,
        PublishedAtUtc = new DateTime(2026, 10, 4, 18, 0, 0, DateTimeKind.Utc),
    };

    // Table 1 is horizontal: a header (10), a "Limits" section (20) and a "Single value" section (30),
    // with part blocks 5 then 6. Block 7 was removed. Table 2 is vertical: a header (40) and a group (50)
    // holding a limits section (51).
    private static readonly PublishedStructure _structure = new(
        [
            new PublishedTableRecord { Id = 1, PublicId = Id(100), DisplayOrder = 0, IsDeleted = false, Title = "Piston parts" },
            new PublishedTableRecord { Id = 2, PublicId = Id(200), DisplayOrder = 1, IsDeleted = false, Title = "Pressures" },
        ],
        [
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
                Id = 30,
                PublicId = Id(130),
                TableId = 1,
                DisplayOrder = 2,
                IsDeleted = false,
                Name = "Single value",
            },
            new PublishedSectionRecord
            {
                Id = 20,
                PublicId = Id(120),
                TableId = 1,
                DisplayOrder = 1,
                IsDeleted = false,
                Name = "Limits",
            },
            new PublishedSectionRecord
            {
                Id = 40,
                PublicId = Id(140),
                TableId = 2,
                DisplayOrder = 0,
                IsDeleted = false,
                Name = "Header",
            },
            new PublishedSectionRecord
            {
                Id = 50,
                PublicId = Id(150),
                TableId = 2,
                DisplayOrder = 1,
                IsDeleted = false,
                Name = "Group",
            },
            new PublishedSectionRecord
            {
                Id = 51,
                PublicId = Id(151),
                TableId = 2,
                ParentSectionId = 50,
                DisplayOrder = 0,
                IsDeleted = false,
                Name = "Limits",
            },
            new PublishedSectionRecord
            {
                Id = 60,
                PublicId = Id(160),
                TableId = 2,
                DisplayOrder = 2,
                IsDeleted = false,
                Name = "Group",
            },
        ],
        [
            new PublishedColumnBlockRecord { Id = 6, PublicId = Id(206), TableId = 1, DisplayOrder = 1, IsDeleted = false, Name = "Part" },
            new PublishedColumnBlockRecord { Id = 5, PublicId = Id(205), TableId = 1, DisplayOrder = 0, IsDeleted = false, Name = "Part" },
            new PublishedColumnBlockRecord { Id = 7, PublicId = Id(207), TableId = 1, DisplayOrder = 2, IsDeleted = true, Name = "Part" },
        ]);

    // The horizontal table as read for a match in block 6: the rows' own cells and block 6's cells.
    private static readonly PublishedLookupCellRecord[] _partColumn =
    [
        Cell(row: 1, section: 10, rowOrder: 0, cell: 1, column: 1, kind: CellKind.Heading, caption: "Description", header: true),
        Cell(row: 1, section: 10, rowOrder: 0, cell: 2, column: 1, block: 6, key: PartNumberKey, header: true),
        Cell(row: 2, section: 10, rowOrder: 1, cell: 4, column: 2, block: 6, kind: CellKind.Heading, caption: "Max", header: true),
        Cell(row: 2, section: 10, rowOrder: 1, cell: 3, column: 1, block: 6, kind: CellKind.Heading, caption: "Min", header: true),
        Cell(row: 4, section: 30, rowOrder: 0, cell: 9, column: 1, block: 6, kind: CellKind.Number),
        Cell(row: 4, section: 30, rowOrder: 0, cell: 8, column: 1),
        Cell(row: 3, section: 20, rowOrder: 0, cell: 7, column: 2, block: 6, kind: CellKind.Number),
        Cell(row: 3, section: 20, rowOrder: 0, cell: 6, column: 1, block: 6, kind: CellKind.Number),
        Cell(row: 3, section: 20, rowOrder: 0, cell: 5, column: 1),
    ];

    private static readonly Dictionary<int, object> _partValues = new()
    {
        [2] = "P-1003",
        [5] = "Bore (mm)",
        [6] = 82.00m,
        [7] = 82.04m,
        [8] = "Material hardness (HV)",
        [9] = 122m,
    };

    [Fact]
    public void ACellInAColumnBlock_BringsThatColumnWithEachRowsOwnValues()
    {
        var match = PublishedLookupAssembler.Assemble(PartHit(), _version, _structure, _partColumn, _partValues);

        Assert.Equal(Id(100), match.Table);
        Assert.Equal("Piston parts", match.Title);
        Assert.Equal("P-1003", match.Column);
        Assert.Equal(["Min", "Max"], match.Headings);
        Assert.Equal([RowId(3), RowId(4)], match.Rows.Keys);

        var limits = match.Rows[RowId(3)];
        Assert.Equal("Limits", limits.Section);
        Assert.Equal(["Bore (mm)"], limits.Values);
        Assert.Equal(["P-1003"], limits.Columns!.Keys);
        Assert.Equal([82.00m, 82.04m], limits.Columns["P-1003"]);

        var single = match.Rows[RowId(4)];
        Assert.Equal("Single value", single.Section);
        Assert.Equal(["Material hardness (HV)"], single.Values);
        Assert.Equal([122m], single.Columns!["P-1003"]);
    }

    [Fact]
    public void AnEmptyCell_KeepsItsPlaceAsNull()
    {
        var values = new Dictionary<int, object>(_partValues);
        values.Remove(6);

        var match = PublishedLookupAssembler.Assemble(PartHit(), _version, _structure, _partColumn, values);

        Assert.Equal([null, 82.04m], match.Rows[RowId(3)].Columns!["P-1003"]);
    }

    [Fact]
    public void ARowWithNothingBesidesTheCellFound_IsLeftOut()
    {
        var values = new Dictionary<int, object> { [2] = "P-1003" };

        var match = PublishedLookupAssembler.Assemble(PartHit(), _version, _structure, _partColumn, values);

        Assert.Empty(match.Rows);
    }

    [Fact]
    public void OtherPartsInTheHeader_AreNotBroughtAlong()
    {
        PublishedLookupCellRecord[] cells =
        [
            .. _partColumn,
            Cell(row: 1, section: 10, rowOrder: 0, cell: 30, column: 1, block: 5, key: PartNumberKey, header: true),
        ];
        var values = new Dictionary<int, object>(_partValues) { [30] = "P-1002" };

        var match = PublishedLookupAssembler.Assemble(PartHit(), _version, _structure, cells, values);

        Assert.Equal("P-1003", match.Column);
        Assert.Equal([RowId(3), RowId(4)], match.Rows.Keys);
        Assert.All(match.Rows.Values, row => Assert.Equal(["P-1003"], row.Columns!.Keys));
    }

    [Fact]
    public void ACellInAVerticalRow_BringsItsSectionAndSubSections()
    {
        PublishedLookupCellRecord[] cells =
        [
            Cell(row: 10, section: 40, rowOrder: 0, cell: 20, column: 1, kind: CellKind.Heading, caption: "Description", header: true),
            Cell(row: 10, section: 40, rowOrder: 0, cell: 21, column: 2, kind: CellKind.Heading, caption: "Min", header: true),
            Cell(row: 10, section: 40, rowOrder: 0, cell: 22, column: 3, kind: CellKind.Heading, caption: "Max", header: true),
            Cell(row: 12, section: 51, rowOrder: 0, cell: 26, column: 3, kind: CellKind.Number),
            Cell(row: 12, section: 51, rowOrder: 0, cell: 25, column: 2, kind: CellKind.Number),
            Cell(row: 12, section: 51, rowOrder: 0, cell: 24, column: 1),
            Cell(row: 11, section: 50, rowOrder: 0, cell: 23, column: 1, key: "group"),
        ];
        var values = new Dictionary<int, object> { [23] = "Oil system", [24] = "Oil pressure (bar)", [25] = 2.5m, [26] = 4.2m };
        var hit = Hit(table: 2, section: 50, row: 11, cell: 23, block: null, header: false);

        Assert.Equal([50, 51], PublishedLookupAssembler.ScopeSections(hit, _structure)!.Order());

        var match = PublishedLookupAssembler.Assemble(hit, _version, _structure, cells, values);

        Assert.Null(match.Column);
        Assert.Equal(["Description", "Min", "Max"], match.Headings);

        // The group's own row holds only the cell that was found, so only the limits row comes back.
        var row = Assert.Single(match.Rows);
        Assert.Equal(RowId(12), row.Key);
        Assert.Equal("Limits", row.Value.Section);
        Assert.Equal(["Oil pressure (bar)", 2.5m, 4.2m], row.Value.Values);
        Assert.Null(row.Value.Columns);
    }

    [Fact]
    public void ACellInAHorizontalRow_BringsTheRowAcrossEveryPart()
    {
        PublishedLookupCellRecord[] cells =
        [
            Cell(row: 1, section: 10, rowOrder: 0, cell: 1, column: 1, kind: CellKind.Heading, caption: "Description", header: true),
            Cell(row: 1, section: 10, rowOrder: 0, cell: 2, column: 1, block: 6, key: PartNumberKey, header: true),
            Cell(row: 1, section: 10, rowOrder: 0, cell: 30, column: 1, block: 5, key: PartNumberKey, header: true),
            Cell(row: 1, section: 10, rowOrder: 0, cell: 31, column: 1, block: 7, key: PartNumberKey, header: true),
            Cell(row: 3, section: 20, rowOrder: 0, cell: 5, column: 1, key: "characteristic"),
            Cell(row: 3, section: 20, rowOrder: 0, cell: 6, column: 1, block: 6, kind: CellKind.Number),
            Cell(row: 3, section: 20, rowOrder: 0, cell: 7, column: 2, block: 6, kind: CellKind.Number),
            Cell(row: 3, section: 20, rowOrder: 0, cell: 32, column: 1, block: 5, kind: CellKind.Number),
            Cell(row: 3, section: 20, rowOrder: 0, cell: 33, column: 2, block: 5, kind: CellKind.Number),
            Cell(row: 3, section: 20, rowOrder: 0, cell: 34, column: 1, block: 7, kind: CellKind.Number),
        ];
        var values = new Dictionary<int, object>
        {
            [2] = "P-1003",
            [30] = "P-1002",
            [31] = "P-removed",
            [5] = "Bore (mm)",
            [6] = 82.00m,
            [7] = 82.04m,
            [32] = 81.99m,
            [33] = 82.03m,
            [34] = 1m,
        };
        var hit = Hit(table: 1, section: 20, row: 3, cell: 5, block: null, header: false);

        var match = PublishedLookupAssembler.Assemble(hit, _version, _structure, cells, values);

        Assert.Null(match.Column);
        Assert.Equal(["Description"], match.Headings);

        var row = Assert.Single(match.Rows).Value;
        Assert.Equal(["Bore (mm)"], row.Values);
        Assert.Equal(["P-1002", "P-1003"], row.Columns!.Keys);
        Assert.Equal([81.99m, 82.03m], row.Columns["P-1002"]);
        Assert.Equal([82.00m, 82.04m], row.Columns["P-1003"]);
    }

    [Fact]
    public void ACellInTheHeader_BringsTheWholeTable()
    {
        var hit = Hit(table: 2, section: 40, row: 10, cell: 20, block: null, header: true);

        Assert.Null(PublishedLookupAssembler.ScopeSections(hit, _structure));
    }

    [Fact]
    public void ACellInARemovedBlockOrSection_IsNotOnTheSheet()
    {
        Assert.True(PublishedLookupAssembler.IsOnSheet(PartHit(), _structure));
        Assert.False(PublishedLookupAssembler.IsOnSheet(Hit(table: 1, section: 10, row: 1, cell: 31, block: 7, header: true), _structure));
        Assert.False(PublishedLookupAssembler.IsOnSheet(Hit(table: 1, section: 99, row: 1, cell: 2, block: null, header: false), _structure));
    }

    private static PublishedLookupHit PartHit()
    {
        return Hit(table: 1, section: 10, row: 1, cell: 2, block: 6, header: true);
    }

    private static PublishedLookupHit Hit(int table, int section, int row, int cell, int? block, bool header)
    {
        return new PublishedLookupHit
        {
            SheetId = 1,
            SheetPublicId = Id(1),
            PhaseCode = "V6",
            SheetTypeId = 3,
            TableId = table,
            SectionId = section,
            RowId = row,
            CellId = cell,
            ColumnBlockId = block,
            IsHeader = header,
        };
    }

    private static PublishedLookupCellRecord Cell(
        int row,
        int section,
        int rowOrder,
        int cell,
        int column,
        int? block = null,
        CellKind kind = CellKind.Text,
        string? caption = null,
        string? key = null,
        bool header = false)
    {
        return new PublishedLookupCellRecord
        {
            RowRevisionId = row * 10,
            RowId = row,
            RowPublicId = RowId(row),
            SectionId = section,
            RowOrder = rowOrder,
            CellId = cell,
            CellPublicId = Id(1000 + cell),
            ColumnBlockId = block,
            Column = column,
            Kind = kind,
            Caption = caption,
            LookupKey = key,
            IsHeader = header,
        };
    }

    private static Guid RowId(int row)
    {
        return Id(500 + row);
    }

    private static Guid Id(int number)
    {
        return new Guid(number, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }
}
