using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Templates;

/// <summary>
/// Stops template deletes that would remove layout sheets already use. Sheet tables, rows and cell values
/// point at the template, its rows and its cells, so deleting those would change how saved sheets read.
/// </summary>
internal static class SheetDataGuard
{
    public static async Task EnsureTemplateUnusedAsync(PuSpecSheetDbContext db, int templateId, CancellationToken cancellationToken)
    {
        if (await db.SheetTables.AnyAsync(table => table.TableTemplateId == templateId, cancellationToken))
        {
            throw new ConflictException("Sheets already use this table, so it can't be deleted.");
        }
    }

    /// <summary>Refuses when sheets have any of <paramref name="sectionIds"/>, or data in their rows.</summary>
    public static async Task EnsureSectionsUnusedAsync(
        PuSpecSheetDbContext db,
        IReadOnlyCollection<int> sectionIds,
        IReadOnlyCollection<int> rowIds,
        string what,
        CancellationToken cancellationToken)
    {
        if (await db.SheetSections.AnyAsync(section => sectionIds.Contains(section.TemplateSectionId), cancellationToken))
        {
            throw new ConflictException($"Sheets already use {what}, so it can't be deleted.");
        }

        await EnsureRowsUnusedAsync(db, rowIds, what, cancellationToken);
    }

    /// <summary>Refuses when sheets have rows from any of <paramref name="rowIds"/>, or values in any of their cells.</summary>
    public static async Task EnsureRowsUnusedAsync(
        PuSpecSheetDbContext db,
        IReadOnlyCollection<int> rowIds,
        string what,
        CancellationToken cancellationToken)
    {
        if (rowIds.Count == 0)
        {
            return;
        }

        if (await db.SheetRows.AnyAsync(row => rowIds.Contains(row.TemplateRowId), cancellationToken))
        {
            throw new ConflictException($"Sheets already have data in {what}, so it can't be deleted.");
        }

        var cellIds = await db.TemplateCells
            .Where(cell => rowIds.Contains(cell.TemplateRowId))
            .Select(cell => cell.Id)
            .ToListAsync(cancellationToken);

        await EnsureCellsUnusedAsync(db, cellIds, what, cancellationToken);
    }

    /// <summary>Refuses when any sheet has a value in one of <paramref name="cellIds"/>.</summary>
    public static async Task EnsureCellsUnusedAsync(
        PuSpecSheetDbContext db,
        IReadOnlyCollection<int> cellIds,
        string what,
        CancellationToken cancellationToken)
    {
        if (cellIds.Count == 0)
        {
            return;
        }

        // Values now hang off sheet cells, and a sheet cell exists for every template cell a sheet uses.
        var used = await db.SheetCells.AnyAsync(cell => cellIds.Contains(cell.TemplateCellId), cancellationToken);

        if (used)
        {
            throw new ConflictException($"Sheets already have data in {what}, so it can't be deleted.");
        }
    }
}
