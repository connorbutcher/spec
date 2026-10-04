using PUSpecSheet.Application.Sheets;

namespace PUSpecSheet.Application.Tests.Sheets;

/// <summary>
/// The all-rows, all-columns guarantee of horizontal tables: every row has a cell for each template cell
/// of its template row, in every column block copy on the table.
/// </summary>
public sealed class SheetCellPlannerTests
{
    // Template: a header row (cells 1 = stub, 2 = in block 10) and a data row (cells 3 = stub, 4 and 5 = in block 10).
    private static readonly (int TemplateCellId, int TemplateRowId, int? TemplateColumnBlockId)[] TemplateCells =
    [
        (1, 100, null),
        (2, 100, 10),
        (3, 200, null),
        (4, 200, 10),
        (5, 200, 10),
    ];

    [Fact]
    public void NewTableWithOneBlockCopy_GivesEveryRowItsOwnCellsAndTheBlockCells()
    {
        var missing = SheetCellPlanner.Missing(
            [(1, 100), (2, 200)],
            TemplateCells,
            [(50, 10)],
            []);

        Assert.Equal(
            [
                (1, 1, (int?)null),
                (1, 2, 50),
                (2, 3, null),
                (2, 4, 50),
                (2, 5, 50),
            ],
            missing.OrderBy(cell => cell.SheetRowId).ThenBy(cell => cell.TemplateCellId));
    }

    [Fact]
    public void AddingABlockCopy_GivesEveryExistingRowItsCellsInIt()
    {
        var rows = new[] { (1, 100), (2, 200), (3, 200), (4, 200) };
        var existing = SheetCellPlanner
            .Missing(rows, TemplateCells, [(50, 10)], [])
            .ToList();

        var missing = SheetCellPlanner.Missing(rows, TemplateCells, [(50, 10), (51, 10)], existing);

        Assert.Equal(
            [(1, 2, 51), (2, 4, 51), (2, 5, 51), (3, 4, 51), (3, 5, 51), (4, 4, 51), (4, 5, 51)],
            missing.OrderBy(cell => cell.SheetRowId).ThenBy(cell => cell.TemplateCellId).Select(cell => (cell.SheetRowId, cell.TemplateCellId, cell.SheetColumnBlockId!.Value)));
    }

    [Fact]
    public void AddingARow_GivesItCellsInEveryBlockCopy()
    {
        var blocks = new[] { (50, 10), (51, 10), (52, 10) };
        var existing = SheetCellPlanner.Missing([(1, 100)], TemplateCells, blocks, []).ToList();

        var missing = SheetCellPlanner.Missing([(1, 100), (2, 200)], TemplateCells, blocks, existing);

        Assert.Equal(
            [(2, 3, (int?)null), (2, 4, 50), (2, 4, 51), (2, 4, 52), (2, 5, 50), (2, 5, 51), (2, 5, 52)],
            missing.OrderBy(cell => cell.TemplateCellId).ThenBy(cell => cell.SheetColumnBlockId));
    }

    [Fact]
    public void Missing_IsEmptyOnceFilled_SoFillingTwiceChangesNothing()
    {
        var rows = new[] { (1, 100), (2, 200) };
        var blocks = new[] { (50, 10), (51, 10) };
        var filled = SheetCellPlanner.Missing(rows, TemplateCells, blocks, []).ToList();

        Assert.Empty(SheetCellPlanner.Missing(rows, TemplateCells, blocks, filled));
    }

    [Fact]
    public void AVerticalTable_HasNoBlocksAndOnlyGetsOwnCells()
    {
        var missing = SheetCellPlanner.Missing(
            [(1, 100)],
            [(1, 100, null), (2, 100, null)],
            [],
            []);

        Assert.Equal([(1, 1, (int?)null), (1, 2, null)], missing.OrderBy(cell => cell.TemplateCellId));
    }

    [Fact]
    public void ABlockCopyOfAnotherTemplateBlock_DoesNotGetThisBlocksCells()
    {
        var missing = SheetCellPlanner.Missing(
            [(1, 200)],
            TemplateCells,
            [(60, 99)],
            []);

        Assert.DoesNotContain(missing, cell => cell.SheetColumnBlockId == 60);
        Assert.Single(missing);
    }
}
