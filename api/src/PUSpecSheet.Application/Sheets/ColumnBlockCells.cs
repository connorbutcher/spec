using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Sheets;

internal static class ColumnBlockCells
{
    /// <summary>
    /// Deletes column blocks nobody but their author ever saw, with their cells and any values typed into
    /// those cells. A cell can't cascade from its block (it already cascades from its row), so everything
    /// that points at the cells goes first.
    /// </summary>
    public static async Task DeleteBlocksAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> blockIds, CancellationToken cancellationToken)
    {
        if (blockIds.Count == 0)
        {
            return;
        }

        await db.TextValues.Where(value => value.SheetCell.SheetColumnBlockId != null && blockIds.Contains(value.SheetCell.SheetColumnBlockId.Value)).ExecuteDeleteAsync(cancellationToken);
        await db.NumericValues.Where(value => value.SheetCell.SheetColumnBlockId != null && blockIds.Contains(value.SheetCell.SheetColumnBlockId.Value)).ExecuteDeleteAsync(cancellationToken);
        await db.DateValues.Where(value => value.SheetCell.SheetColumnBlockId != null && blockIds.Contains(value.SheetCell.SheetColumnBlockId.Value)).ExecuteDeleteAsync(cancellationToken);
        await db.BooleanValues.Where(value => value.SheetCell.SheetColumnBlockId != null && blockIds.Contains(value.SheetCell.SheetColumnBlockId.Value)).ExecuteDeleteAsync(cancellationToken);
        await db.OptionValues.Where(value => value.SheetCell.SheetColumnBlockId != null && blockIds.Contains(value.SheetCell.SheetColumnBlockId.Value)).ExecuteDeleteAsync(cancellationToken);
        await db.SheetCells.Where(cell => cell.SheetColumnBlockId != null && blockIds.Contains(cell.SheetColumnBlockId.Value)).ExecuteDeleteAsync(cancellationToken);
        await db.SheetColumnBlocks.Where(block => blockIds.Contains(block.Id)).ExecuteDeleteAsync(cancellationToken);
    }
}
