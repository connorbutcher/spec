using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Templates.Queries;

/// <summary>The cells of template rows.</summary>
public static class TemplateCellQueries
{
    /// <summary>The cell with its row and the row's section, tracked for editing; null when there is no such cell.</summary>
    public static async Task<DbTemplateCell?> FindForEditAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.TemplateCells
            .Include(candidate => candidate.TemplateRow)
            .ThenInclude(row => row.TemplateSection)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    /// <summary>The cell with its cell type, row and the row's section, tracked for editing; null when there is no such cell.</summary>
    public static async Task<DbTemplateCell?> FindWithTypeForEditAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.TemplateCells
            .Include(candidate => candidate.CellType)
            .Include(candidate => candidate.TemplateRow)
            .ThenInclude(row => row.TemplateSection)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    /// <summary>Whether a cell other than <paramref name="exceptId"/> starts at the column of the row (and column block).</summary>
    public static async Task<bool> ColumnTakenAsync(PuSpecSheetDbContext db, int rowId, int? columnBlockId, int column, int exceptId, CancellationToken cancellationToken)
    {
        return await db.TemplateCells.AnyAsync(
            other => other.TemplateRowId == rowId
                && other.TemplateColumnBlockId == columnBlockId
                && other.Column == column
                && other.Id != exceptId,
            cancellationToken);
    }

    /// <summary>
    /// Moves the row's cells (in the column block, if any) from the column on one to the right, in one
    /// statement so the one-cell-per-column index is only checked once they've all moved.
    /// </summary>
    public static async Task ShiftRightAsync(PuSpecSheetDbContext db, int rowId, int? columnBlockId, int fromColumn, CancellationToken cancellationToken)
    {
        await db.TemplateCells
            .Where(cell => cell.TemplateRowId == rowId
                && cell.TemplateColumnBlockId == columnBlockId
                && cell.Column >= fromColumn)
            .ExecuteUpdateAsync(setters => setters.SetProperty(cell => cell.Column, cell => cell.Column + 1), cancellationToken);
    }

    /// <summary>Deletes the column block's cells straight in the database.</summary>
    public static async Task DeleteInColumnBlockAsync(PuSpecSheetDbContext db, int columnBlockId, CancellationToken cancellationToken)
    {
        await db.TemplateCells
            .Where(cell => cell.TemplateColumnBlockId == columnBlockId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>The cells asked for, read only.</summary>
    public static async Task<List<DbTemplateCell>> ListAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        return await db.TemplateCells.AsNoTracking().Where(cell => ids.Contains(cell.Id)).ToListAsync(cancellationToken);
    }

    /// <summary>Whether any template cell has the lookup key.</summary>
    public static async Task<bool> LookupKeyExistsAsync(PuSpecSheetDbContext db, string key, CancellationToken cancellationToken)
    {
        return await db.TemplateCells.AnyAsync(cell => cell.LookupKey == key, cancellationToken);
    }
}
