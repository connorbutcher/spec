using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// Clears a development database down to its reference data so the showcase can be built on it: every
/// sheet with all its versions, drafts and values, every table template, and every cell type that isn't
/// one of the built-in ones the migrations add. Phases, sheet types, users and permissions are kept. It
/// runs in one transaction, so a failure leaves the database as it was.
/// </summary>
public static class SampleDataQueries
{
    // The cell types the migrations seed (see CellTypeConfiguration). Anything above is user-made.
    private const int LastBuiltInCellTypeId = 7;

    /// <summary>
    /// Deletes every sheet with all its versions, drafts and values, every table template, and every cell
    /// type that isn't one of the built-in ones the migrations add. Phases, sheet types, users and permissions
    /// are kept. It runs in one transaction, so a failure leaves the database as it was.
    /// </summary>
    public static async Task ClearAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        // Children before parents: several of these foreign keys don't cascade, as SQL Server allows only
        // one cascade path to a table.
        await db.TextValues.ExecuteDeleteAsync(cancellationToken);
        await db.NumericValues.ExecuteDeleteAsync(cancellationToken);
        await db.DateValues.ExecuteDeleteAsync(cancellationToken);
        await db.BooleanValues.ExecuteDeleteAsync(cancellationToken);
        await db.OptionValues.ExecuteDeleteAsync(cancellationToken);
        await db.CellSettings.ExecuteDeleteAsync(cancellationToken);
        await db.LabelledTextValues.ExecuteDeleteAsync(cancellationToken);
        await db.SheetRowRevisions.ExecuteDeleteAsync(cancellationToken);
        await db.SheetCells.ExecuteDeleteAsync(cancellationToken);
        await db.SheetRows.ExecuteDeleteAsync(cancellationToken);
        await db.SheetColumnBlockRevisions.ExecuteDeleteAsync(cancellationToken);
        await db.SheetSectionRevisions.ExecuteDeleteAsync(cancellationToken);
        await db.SheetTableRevisions.ExecuteDeleteAsync(cancellationToken);
        await db.SheetColumnBlocks.ExecuteDeleteAsync(cancellationToken);
        await db.SheetSections.ExecuteDeleteAsync(cancellationToken);
        await db.SheetTables.ExecuteDeleteAsync(cancellationToken);
        await db.SheetVersions.ExecuteDeleteAsync(cancellationToken);
        await db.Sheets.ExecuteDeleteAsync(cancellationToken);

        await db.TemplateCells.ExecuteDeleteAsync(cancellationToken);
        await db.TemplateColumnBlocks.ExecuteDeleteAsync(cancellationToken);
        await db.TemplateRows.ExecuteDeleteAsync(cancellationToken);
        await db.TemplateSections.ExecuteDeleteAsync(cancellationToken);
        await db.TableTemplateVersions.ExecuteDeleteAsync(cancellationToken);
        await db.TableTemplates.ExecuteDeleteAsync(cancellationToken);

        await db.CellTypes.Where(cellType => cellType.Id > LastBuiltInCellTypeId).ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        db.ChangeTracker.Clear();
    }
}
