using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Seeding;

public sealed class SampleDataService : ISampleDataService
{
    // The cell types the migrations seed (see CellTypeConfiguration). Anything above is user-made.
    private const int LastBuiltInCellTypeId = 7;

    private readonly PuSpecSheetDbContext _db;

    public SampleDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        // Children before parents: several of these foreign keys don't cascade, as SQL Server allows only
        // one cascade path to a table.
        await _db.TextValues.ExecuteDeleteAsync(cancellationToken);
        await _db.NumericValues.ExecuteDeleteAsync(cancellationToken);
        await _db.DateValues.ExecuteDeleteAsync(cancellationToken);
        await _db.BooleanValues.ExecuteDeleteAsync(cancellationToken);
        await _db.OptionValues.ExecuteDeleteAsync(cancellationToken);
        await _db.CellSettings.ExecuteDeleteAsync(cancellationToken);
        await _db.LabelledTextValues.ExecuteDeleteAsync(cancellationToken);
        await _db.SheetRowRevisions.ExecuteDeleteAsync(cancellationToken);
        await _db.SheetCells.ExecuteDeleteAsync(cancellationToken);
        await _db.SheetRows.ExecuteDeleteAsync(cancellationToken);
        await _db.SheetColumnBlockRevisions.ExecuteDeleteAsync(cancellationToken);
        await _db.SheetSectionRevisions.ExecuteDeleteAsync(cancellationToken);
        await _db.SheetTableRevisions.ExecuteDeleteAsync(cancellationToken);
        await _db.SheetColumnBlocks.ExecuteDeleteAsync(cancellationToken);
        await _db.SheetSections.ExecuteDeleteAsync(cancellationToken);
        await _db.SheetTables.ExecuteDeleteAsync(cancellationToken);
        await _db.SheetVersions.ExecuteDeleteAsync(cancellationToken);
        await _db.SheetIdentifierValues.ExecuteDeleteAsync(cancellationToken);
        await _db.Sheets.ExecuteDeleteAsync(cancellationToken);

        await _db.TemplateCells.ExecuteDeleteAsync(cancellationToken);
        await _db.TemplateColumnBlocks.ExecuteDeleteAsync(cancellationToken);
        await _db.TemplateRows.ExecuteDeleteAsync(cancellationToken);
        await _db.TemplateSections.ExecuteDeleteAsync(cancellationToken);
        await _db.TableTemplateVersions.ExecuteDeleteAsync(cancellationToken);
        await _db.TableTemplates.ExecuteDeleteAsync(cancellationToken);

        await _db.CellTypes.Where(cellType => cellType.Id > LastBuiltInCellTypeId).ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }
}
