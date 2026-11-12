using PUSpecSheet.Application.Persistence.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

/// <summary>Loads the drafts of every kind that one publish or submission takes from a sheet, tracked.</summary>
public sealed class SheetDraftSetLoader
{
    private readonly ISheetTableRevisionRepository tables;
    private readonly ISheetSectionRevisionRepository sections;
    private readonly ISheetRowRevisionRepository rows;
    private readonly ISheetColumnBlockRevisionRepository columnBlocks;

    public SheetDraftSetLoader(
        ISheetTableRevisionRepository tables,
        ISheetSectionRevisionRepository sections,
        ISheetRowRevisionRepository rows,
        ISheetColumnBlockRevisionRepository columnBlocks)
    {
        this.tables = tables;
        this.sections = sections;
        this.rows = rows;
        this.columnBlocks = columnBlocks;
    }

    public async Task<SheetDraftSet> LoadAsync(int sheetId, IDraftSelection selection, CancellationToken cancellationToken)
    {
        return new SheetDraftSet(
            await tables.ListSelectedForEditAsync(sheetId, selection, cancellationToken),
            await sections.ListSelectedForEditAsync(sheetId, selection, cancellationToken),
            await rows.ListSelectedForEditAsync(sheetId, selection, cancellationToken),
            await columnBlocks.ListSelectedForEditAsync(sheetId, selection, cancellationToken));
    }
}
