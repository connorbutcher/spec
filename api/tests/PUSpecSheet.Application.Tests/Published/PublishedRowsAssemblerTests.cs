using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Published;
using PUSpecSheet.Contracts.Published;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Tests.Published;

/// <summary>
/// Rows by identifier: a vertical table's row is its values, and a horizontal table's row also has its
/// values for each part under the part number at the top of the part's columns.
/// </summary>
public sealed class PublishedRowsAssemblerTests
{
    private static readonly ResolvedSheetVersion Version = new(1, Id(1), 2, new DateTime(2026, 10, 4, 18, 0, 0, DateTimeKind.Utc));

    // Table 1 is horizontal: a header (10) and a "Limits" section (20), with part blocks 5, 6 and 8.
    // Block 7 was removed. Table 2 is vertical: a header (40) and a "Limits" section (50).
    private static readonly PublishedStructure Structure = new(
        [
            new PublishedTableRecord(1, Id(100), 0, false, "Piston parts"),
            new PublishedTableRecord(2, Id(200), 1, false, "Pressures"),
        ],
        [
            new PublishedSectionRecord(10, Id(110), 1, null, 0, false, "Header"),
            new PublishedSectionRecord(20, Id(120), 1, null, 1, false, "Limits"),
            new PublishedSectionRecord(40, Id(140), 2, null, 0, false, "Header"),
            new PublishedSectionRecord(50, Id(150), 2, null, 1, false, "Limits"),
        ],
        [
            new PublishedColumnBlockRecord(5, Id(205), 1, 0, false, "Part"),
            new PublishedColumnBlockRecord(6, Id(206), 1, 1, false, "Part"),
            new PublishedColumnBlockRecord(7, Id(207), 1, 2, true, "Part"),
            new PublishedColumnBlockRecord(8, Id(208), 1, 3, false, "Part"),
        ]);

    private static readonly PublishedLookupCellRecord[] Cells =
    [
        Cell(row: 1, section: 10, cell: 1, column: 1, kind: CellKind.Heading, header: true),
        Cell(row: 1, section: 10, cell: 2, column: 1, block: 5, header: true),
        Cell(row: 1, section: 10, cell: 3, column: 1, block: 6, header: true),
        Cell(row: 1, section: 10, cell: 4, column: 1, block: 7, header: true),
        Cell(row: 1, section: 10, cell: 5, column: 1, block: 8, header: true),
        Cell(row: 2, section: 10, cell: 6, column: 1, block: 5, kind: CellKind.Heading, header: true),
        Cell(row: 3, section: 20, cell: 10, column: 1),
        Cell(row: 3, section: 20, cell: 12, column: 2, block: 5, kind: CellKind.Number),
        Cell(row: 3, section: 20, cell: 11, column: 1, block: 5, kind: CellKind.Number),
        Cell(row: 3, section: 20, cell: 13, column: 1, block: 6, kind: CellKind.Number),
        Cell(row: 3, section: 20, cell: 14, column: 2, block: 6, kind: CellKind.Number),
        Cell(row: 3, section: 20, cell: 15, column: 1, block: 7, kind: CellKind.Number),
        Cell(row: 3, section: 20, cell: 16, column: 1, block: 8, kind: CellKind.Number),
        Cell(row: 9, section: 40, cell: 30, column: 1, kind: CellKind.Heading, header: true),
        Cell(row: 10, section: 50, cell: 31, column: 1),
        Cell(row: 10, section: 50, cell: 32, column: 2, kind: CellKind.Number),
        Cell(row: 10, section: 50, cell: 33, column: 3, kind: CellKind.Number),
    ];

    private static readonly Dictionary<int, object> Values = new()
    {
        [2] = "P-1002",
        [3] = "P-1003",
        [4] = "P-removed",
        [5] = "p-1002",
        [10] = "Bore (mm)",
        [11] = 81.99m,
        [12] = 82.03m,
        [13] = 82.00m,
        [15] = 1m,
        [16] = 5m,
        [31] = "Oil pressure (bar)",
        [33] = 4.2m,
    };

    [Fact]
    public void AVerticalRow_IsItsValuesWithNullForAnEmptyCell()
    {
        var rows = Assemble(PublishedRowsSelection.Parse(null, null));

        var row = rows.Rows[RowId(10)];
        Assert.Equal("Limits", row.Section);
        Assert.Equal(["Oil pressure (bar)", null, 4.2m], row.Values);
        Assert.Null(row.Columns);
    }

    [Fact]
    public void AHorizontalRow_HasItsValuesForEachPartUnderThePartNumber()
    {
        var rows = Assemble(PublishedRowsSelection.Parse(null, null));

        var row = rows.Rows[RowId(3)];
        Assert.Equal(["Bore (mm)"], row.Values);
        Assert.Equal([81.99m, 82.03m], row.Columns!["P-1002"]);
        Assert.Equal([82.00m, null], row.Columns["P-1003"]);

        // The removed part is gone, and a second part with a number already used goes by its identifier.
        Assert.Equal(["P-1002", "P-1003", Id(208).ToString()], row.Columns.Keys);
        Assert.Equal([5m], row.Columns[Id(208).ToString()]);
    }

    [Fact]
    public void EveryRow_LeavesOutRowsOfHeadingsAndKeepsSheetOrder()
    {
        var rows = Assemble(PublishedRowsSelection.Parse(null, null));

        Assert.Equal([RowId(1), RowId(3), RowId(10)], rows.Rows.Keys);
        Assert.Empty(rows.Missing);
    }

    [Fact]
    public void NamedRows_AreTheOnlyOnesReturnedAndUnknownOnesAreListed()
    {
        var selection = PublishedRowsSelection.Parse($"{RowId(10)},{Id(999)}", null);
        var cells = Cells.Where(cell => cell.RowId == 10).ToList();

        var rows = PublishedRowsAssembler.Assemble(Version, selection, Structure, cells, [], Values);

        Assert.Equal([RowId(10)], rows.Rows.Keys);
        Assert.Equal([Id(999)], rows.Missing);
    }

    [Fact]
    public void NamedColumns_KeepOnlyThoseParts()
    {
        var rows = Assemble(PublishedRowsSelection.Parse(null, "p-1003"));

        Assert.Equal(["P-1003"], rows.Rows[RowId(3)].Columns!.Keys);
        Assert.Null(rows.Rows[RowId(10)].Columns);
    }

    [Fact]
    public void ASelectionsKey_IsTheSameInAnyOrderAndDifferentForADifferentQuestion()
    {
        var one = PublishedRowsSelection.Parse($"{RowId(3)},{RowId(10)}", "P-1003,P-1002");
        var other = PublishedRowsSelection.From(new PublishedRowsQueryRequest([RowId(10), RowId(3)], ["p-1002", "P-1003"]));

        Assert.Equal(one.Key, other.Key);
        Assert.Equal("rows-all", PublishedRowsSelection.Parse(null, null).Key);
        Assert.NotEqual(one.Key, PublishedRowsSelection.Parse($"{RowId(3)},{RowId(10)}", null).Key);
        Assert.NotEqual(PublishedRowsSelection.Parse(null, null).Key, PublishedRowsSelection.Parse(null, "P-1003").Key);
        Assert.Throws<InvalidRequestException>(() => PublishedRowsSelection.Parse("not-an-id", null));
    }

    private static PublishedRowsDto Assemble(PublishedRowsSelection selection)
    {
        return PublishedRowsAssembler.Assemble(Version, selection, Structure, Cells, Cells, Values);
    }

    private static PublishedLookupCellRecord Cell(
        int row,
        int section,
        int cell,
        int column,
        int? block = null,
        CellKind kind = CellKind.Text,
        bool header = false)
    {
        return new PublishedLookupCellRecord(row * 10, row, RowId(row), section, row, cell, Id(1000 + cell), block, column, kind, null, null, header);
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
