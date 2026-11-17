using PUSpecSheet.DataAccess.Seeding.ReferenceData;
using PUSpecSheet.DataAccess.Sheets;
using PUSpecSheet.DataAccess.Sheets.Editing;
using PUSpecSheet.DataAccess.Sheets.Reading;
using PUSpecSheet.DataAccess.SheetTypes.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Seeding.Sheets;

/// <summary>
/// Opens a sheet for a <see cref="SheetEditor"/>: a phase's one sheet of a sheet type, creating it as the app
/// does the first time, or a sheet by id.
/// </summary>
public sealed class SheetEditorFactory
{
    private readonly PuSpecSheetDbContext db;
    private readonly ISheetsService sheets;
    private readonly ISheetTablesService tables;
    private readonly ISheetSectionsService sections;
    private readonly ISheetRowsService rows;
    private readonly ISheetColumnBlocksService columnBlocks;
    private readonly ISheetCellSettingsService cellSettings;

    public SheetEditorFactory(
        PuSpecSheetDbContext db,
        ISheetsService sheets,
        ISheetTablesService tables,
        ISheetSectionsService sections,
        ISheetRowsService rows,
        ISheetColumnBlocksService columnBlocks,
        ISheetCellSettingsService cellSettings)
    {
        this.db = db;
        this.sheets = sheets;
        this.tables = tables;
        this.sections = sections;
        this.rows = rows;
        this.columnBlocks = columnBlocks;
        this.cellSettings = cellSettings;
    }

    public async Task<SheetEditor> OpenAsync(int phaseId, string sheetTypeName, ShowcaseCellTypeCatalog cellTypes, CancellationToken cancellationToken)
    {
        var sheetTypeId = await SheetTypeQueries.GetIdByNameAsync(db, sheetTypeName, cancellationToken);
        var sheet = await sheets.OpenAsync(phaseId, sheetTypeId, cancellationToken);
        return new SheetEditor(sheet, cellTypes, sheets, tables, sections, rows, columnBlocks, cellSettings);
    }

    /// <summary>Opens the sheet <paramref name="sheetId"/>, one of several of its type in a phase, as the current user sees it.</summary>
    public async Task<SheetEditor> OpenSheetAsync(int sheetId, ShowcaseCellTypeCatalog cellTypes, CancellationToken cancellationToken)
    {
        var sheet = await sheets.GetAsync(sheetId, SheetViewPoint.Live, cancellationToken);
        return new SheetEditor(sheet, cellTypes, sheets, tables, sections, rows, columnBlocks, cellSettings);
    }
}
