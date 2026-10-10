using PUSpecSheet.Application.Published;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Tests.Published;

/// <summary>
/// A linked dropdown that gives its choice's row is answered with the row the value was picked from.
/// </summary>
public sealed class PublishedLinkedRowsTests
{
    private static readonly ResolvedSheetVersion Version = new(1, Id(1), 2, new DateTime(2026, 10, 10, 9, 0, 0, DateTimeKind.Utc));

    // One table: a "Limits" section (20) and a "Removed" section that is no longer on the sheet (30 is not listed).
    private static readonly PublishedStructure Structure = new(
        [new PublishedTableRecord(1, Id(100), 0, false, "Piston parts")],
        [
            new PublishedSectionRecord(20, Id(120), 1, null, 1, false, "Limits"),
            new PublishedSectionRecord(21, Id(121), 1, null, 0, false, "Limits"),
        ],
        []);

    [Fact]
    public void EachValueOfAColumn_BelongsToTheRowThatHoldsIt()
    {
        var column = new[] { Cell(row: 3, section: 20, cell: 10), Cell(row: 4, section: 20, cell: 11) };
        var values = new Dictionary<int, object> { [10] = "Bore (mm)", [11] = "Stroke (mm)" };

        var rows = PublishedLinkedRows.RowByOption(Structure, column, values);

        Assert.Equal(RowId(3), rows["Bore (mm)"]);
        Assert.Equal(RowId(4), rows["Stroke (mm)"]);
    }

    [Fact]
    public void AValueHeldByTwoRows_MeansTheFirstOfThemOnTheSheet()
    {
        // Section 21 comes before section 20, whatever order the cells were read in.
        var column = new[] { Cell(row: 3, section: 20, cell: 10), Cell(row: 4, section: 21, cell: 11) };
        var values = new Dictionary<int, object> { [10] = "Bore (mm)", [11] = "Bore (mm)" };

        var rows = PublishedLinkedRows.RowByOption(Structure, column, values);

        Assert.Equal(RowId(4), rows["Bore (mm)"]);
    }

    [Fact]
    public void EmptyCells_AndCellsInARemovedSection_AreNoChoice()
    {
        var column = new[] { Cell(row: 3, section: 20, cell: 10), Cell(row: 5, section: 30, cell: 12) };
        var values = new Dictionary<int, object> { [12] = "Gone" };

        Assert.Empty(PublishedLinkedRows.RowByOption(Structure, column, values));
    }

    [Fact]
    public void AValue_IsMatchedAsTheTextALinkedDropdownStores()
    {
        Assert.Equal("Bore (mm)", PublishedLinkedRows.OptionText(" Bore (mm) "));
        Assert.Equal("12.5", PublishedLinkedRows.OptionText(12.5m));
        Assert.Equal("2026-10-08", PublishedLinkedRows.OptionText(new DateOnly(2026, 10, 8)));
    }

    [Fact]
    public void ARowsLinkedRows_AreKeyedByTheValuePicked()
    {
        var cells = new[] { Cell(row: 7, section: 20, cell: 40, kind: CellKind.LinkedDropdown), Cell(row: 7, section: 20, cell: 41) };
        var values = new Dictionary<int, object> { [40] = "Bore (mm)", [41] = "Metrology" };
        var linked = new Dictionary<int, Guid> { [40] = RowId(3) };

        var result = PublishedLinkedRows.Of(cells, values, linked);

        Assert.Equal(new Dictionary<string, Guid> { ["Bore (mm)"] = RowId(3) }, result);
    }

    [Fact]
    public void ARowWithNoLinkedCell_HasNoLinkedRowsAtAll()
    {
        var cells = new[] { Cell(row: 7, section: 20, cell: 41) };
        var values = new Dictionary<int, object> { [41] = "Metrology" };

        Assert.Null(PublishedLinkedRows.Of(cells, values, null));
        Assert.Null(PublishedLinkedRows.Of(cells, values, new Dictionary<int, Guid>()));
        Assert.Null(PublishedLinkedRows.Of(cells, values, new Dictionary<int, Guid> { [99] = RowId(3) }));
    }

    [Fact]
    public void RowsByIdentifier_CarryTheLinkedRows()
    {
        var cells = new[] { Cell(row: 7, section: 20, cell: 40, kind: CellKind.LinkedDropdown), Cell(row: 7, section: 20, cell: 41, column: 2) };
        var values = new Dictionary<int, object> { [40] = "Bore (mm)", [41] = "Metrology" };
        var linked = new Dictionary<int, Guid> { [40] = RowId(3) };

        var withLink = PublishedRowsAssembler.Assemble(Version, PublishedRowsSelection.One(RowId(7), null), Structure, cells, [], values, linked);
        var without = PublishedRowsAssembler.Assemble(Version, PublishedRowsSelection.One(RowId(7), null), Structure, cells, [], values);

        Assert.Equal(RowId(3), withLink.Rows[RowId(7)].LinkedRows!["Bore (mm)"]);
        Assert.Null(without.Rows[RowId(7)].LinkedRows);
    }

    private static PublishedLookupCellRecord Cell(int row, int section, int cell, int column = 1, CellKind kind = CellKind.Text)
    {
        return new PublishedLookupCellRecord(row * 10, row, RowId(row), section, row, cell, Id(1000 + cell), null, column, kind, null, null, false);
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
