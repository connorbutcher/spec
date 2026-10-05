using PUSpecSheet.Application.Published;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Tests.Published;

/// <summary>
/// What a caller receives for a cell a lookup found, as rows by identifier: the column of a cell in a column block, or the rows
/// of a cell among a row's own cells, on a horizontal table and a vertical one.
/// </summary>
public sealed class PublishedLookupAssemblerTests
{
    private const string PartNumberKey = "partNumber";

    private static readonly ResolvedSheetVersion Version = new(1, Id(1), 2, new DateTime(2026, 10, 4, 18, 0, 0, DateTimeKind.Utc));

    // Table 1 is horizontal: a header (10), a "Limits" section (20) and a "Single value" section (30),
    // with part blocks 5 then 6. Block 7 was removed. Table 2 is vertical: a header (40) and a group (50)
    // holding a limits section (51).
    private static readonly PublishedStructure Structure = new(
        [
            new PublishedTableRecord(1, Id(100), 0, false, "Piston parts"),
            new PublishedTableRecord(2, Id(200), 1, false, "Pressures"),
        ],
        [
            new PublishedSectionRecord(10, Id(110), 1, null, 0, false, "Header"),
            new PublishedSectionRecord(30, Id(130), 1, null, 2, false, "Single value"),
            new PublishedSectionRecord(20, Id(120), 1, null, 1, false, "Limits"),
            new PublishedSectionRecord(40, Id(140), 2, null, 0, false, "Header"),
            new PublishedSectionRecord(50, Id(150), 2, null, 1, false, "Group"),
            new PublishedSectionRecord(51, Id(151), 2, 50, 0, false, "Limits"),
            new PublishedSectionRecord(60, Id(160), 2, null, 2, false, "Group"),
        ],
        [
            new PublishedColumnBlockRecord(6, Id(206), 1, 1, false, "Part"),
            new PublishedColumnBlockRecord(5, Id(205), 1, 0, false, "Part"),
            new PublishedColumnBlockRecord(7, Id(207), 1, 2, true, "Part"),
        ]);

    // The horizontal table as read for a match in block 6: the rows' own cells and block 6's cells.
    private static readonly PublishedLookupCellRecord[] PartColumn =
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

    private static readonly Dictionary<int, object> PartValues = new()
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
        var match = PublishedLookupAssembler.Assemble(PartHit(), Version, Structure, PartColumn, PartValues);

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
        var values = new Dictionary<int, object>(PartValues);
        values.Remove(6);

        var match = PublishedLookupAssembler.Assemble(PartHit(), Version, Structure, PartColumn, values);

        Assert.Equal([null, 82.04m], match.Rows[RowId(3)].Columns!["P-1003"]);
    }

    [Fact]
    public void ARowWithNothingBesidesTheCellFound_IsLeftOut()
    {
        var values = new Dictionary<int, object> { [2] = "P-1003" };

        var match = PublishedLookupAssembler.Assemble(PartHit(), Version, Structure, PartColumn, values);

        Assert.Empty(match.Rows);
    }

    [Fact]
    public void OtherPartsInTheHeader_AreNotBroughtAlong()
    {
        PublishedLookupCellRecord[] cells =
        [
            .. PartColumn,
            Cell(row: 1, section: 10, rowOrder: 0, cell: 30, column: 1, block: 5, key: PartNumberKey, header: true),
        ];
        var values = new Dictionary<int, object>(PartValues) { [30] = "P-1002" };

        var match = PublishedLookupAssembler.Assemble(PartHit(), Version, Structure, cells, values);

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

        Assert.Equal([50, 51], PublishedLookupAssembler.ScopeSections(hit, Structure)!.Order());

        var match = PublishedLookupAssembler.Assemble(hit, Version, Structure, cells, values);

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

        var match = PublishedLookupAssembler.Assemble(hit, Version, Structure, cells, values);

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

        Assert.Null(PublishedLookupAssembler.ScopeSections(hit, Structure));
    }

    [Fact]
    public void ACellInARemovedBlockOrSection_IsNotOnTheSheet()
    {
        Assert.True(PublishedLookupAssembler.IsOnSheet(PartHit(), Structure));
        Assert.False(PublishedLookupAssembler.IsOnSheet(Hit(table: 1, section: 10, row: 1, cell: 31, block: 7, header: true), Structure));
        Assert.False(PublishedLookupAssembler.IsOnSheet(Hit(table: 1, section: 99, row: 1, cell: 2, block: null, header: false), Structure));
    }

    private static PublishedLookupHit PartHit()
    {
        return Hit(table: 1, section: 10, row: 1, cell: 2, block: 6, header: true);
    }

    private static PublishedLookupHit Hit(int table, int section, int row, int cell, int? block, bool header)
    {
        return new PublishedLookupHit(1, Id(1), "V6", 3, table, section, row, cell, block, header);
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
        return new PublishedLookupCellRecord(row * 10, row, RowId(row), section, rowOrder, cell, Id(1000 + cell), block, column, kind, caption, key, header);
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
