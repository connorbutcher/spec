using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Sheets.Drafts;

/// <summary>Loads the drafts of every kind that one publish or submission takes from a sheet, tracked.</summary>
public sealed class SheetDraftSetLoader
{
    private readonly PuSpecSheetDbContext _db;

    public SheetDraftSetLoader(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<SheetDraftSet> LoadAsync(int sheetId, DraftSelection selection, CancellationToken cancellationToken)
    {
        return new SheetDraftSet
        {
            Tables = await _db.SheetTableRevisions.ListSelectedForEditAsync(sheetId, selection, cancellationToken),
            Sections = await _db.SheetSectionRevisions.ListSelectedForEditAsync(sheetId, selection, cancellationToken),
            Rows = await _db.SheetRowRevisions.ListSelectedForEditAsync(sheetId, selection, cancellationToken),
            ColumnBlocks = await _db.SheetColumnBlockRevisions.ListSelectedForEditAsync(sheetId, selection, cancellationToken),
        };
    }
}
