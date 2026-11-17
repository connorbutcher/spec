using PUSpecSheet.DataAccess.Seeding.ReferenceData;
using PUSpecSheet.DataAccess.Sheets;
using PUSpecSheet.DataAccess.Sheets.Editing;
using PUSpecSheet.DataAccess.Sheets.Reading;
using PUSpecSheet.DataAccess.SheetTypes;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Seeding.Sheets;

/// <summary>
/// Opens a sheet for a <see cref="SheetEditor"/>: a phase's one sheet of a sheet type, creating it as the app
/// does the first time, or a sheet by id.
/// </summary>
public sealed class SheetEditorFactory
{
    private readonly PuSpecSheetDbContext _db;
    private readonly ISheetsService _sheets;
    private readonly ISheetTablesService _tables;
    private readonly ISheetSectionsService _sections;
    private readonly ISheetRowsService _rows;
    private readonly ISheetColumnBlocksService _columnBlocks;
    private readonly ISheetCellSettingsService _cellSettings;
    private readonly ISheetTypeDataService _sheetTypes;

    public SheetEditorFactory(
        PuSpecSheetDbContext db,
        ISheetsService sheets,
        ISheetTablesService tables,
        ISheetSectionsService sections,
        ISheetRowsService rows,
        ISheetColumnBlocksService columnBlocks,
        ISheetCellSettingsService cellSettings,
        ISheetTypeDataService sheetTypes)
    {
        _db = db;
        _sheets = sheets;
        _tables = tables;
        _sections = sections;
        _rows = rows;
        _columnBlocks = columnBlocks;
        _cellSettings = cellSettings;
        _sheetTypes = sheetTypes;
    }

    public async Task<SheetEditor> OpenAsync(int phaseId, string sheetTypeName, ShowcaseCellTypeCatalog cellTypes, CancellationToken cancellationToken)
    {
        var sheetTypeId = await _sheetTypes.GetIdByNameAsync(sheetTypeName, cancellationToken);
        var sheet = await _sheets.OpenAsync(phaseId, sheetTypeId, cancellationToken);
        return new SheetEditor(sheet, cellTypes, _sheets, _tables, _sections, _rows, _columnBlocks, _cellSettings);
    }

    /// <summary>Opens the sheet <paramref name="sheetId"/>, one of several of its type in a phase, as the current user sees it.</summary>
    public async Task<SheetEditor> OpenSheetAsync(int sheetId, ShowcaseCellTypeCatalog cellTypes, CancellationToken cancellationToken)
    {
        var sheet = await _sheets.GetAsync(sheetId, SheetViewPoint.Live, cancellationToken);
        return new SheetEditor(sheet, cellTypes, _sheets, _tables, _sections, _rows, _columnBlocks, _cellSettings);
    }
}
