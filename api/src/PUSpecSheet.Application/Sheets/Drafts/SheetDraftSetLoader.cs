using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Drafts;

/// <summary>Loads the drafts of every kind that one publish or submission takes from a sheet, tracked.</summary>
public sealed class SheetDraftSetLoader
{
    private readonly PuSpecSheetDbContext db;

    public SheetDraftSetLoader(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task<SheetDraftSet> LoadAsync(int sheetId, DraftSelection selection, CancellationToken cancellationToken)
    {
        return new SheetDraftSet(
            await db.SheetTableRevisions.ListSelectedForEditAsync(sheetId, selection, cancellationToken),
            await db.SheetSectionRevisions.ListSelectedForEditAsync(sheetId, selection, cancellationToken),
            await db.SheetRowRevisions.ListSelectedForEditAsync(sheetId, selection, cancellationToken),
            await db.SheetColumnBlockRevisions.ListSelectedForEditAsync(sheetId, selection, cancellationToken));
    }
}
