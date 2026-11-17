using PUSpecSheet.DataAccess.Seeding.ReferenceData;
using PUSpecSheet.DataAccess.Sheets;
using PUSpecSheet.DataAccess.Sheets.Editing;
using PUSpecSheet.DataAccess.Sheets.Reading;
using PUSpecSheet.DataAccess.SheetTypes;

namespace PUSpecSheet.DataAccess.Seeding.Sheets;

public sealed class SheetEditorFactory
{
    private readonly ISheetsService _sheets;
    private readonly ISheetTablesService _tables;
    private readonly ISheetSectionsService _sections;
    private readonly ISheetRowsService _rows;
    private readonly ISheetColumnBlocksService _columnBlocks;
    private readonly ISheetCellSettingsService _cellSettings;
    private readonly ISheetTypeDataService _sheetTypes;

    public SheetEditorFactory(
        ISheetsService sheets,
        ISheetTablesService tables,
        ISheetSectionsService sections,
        ISheetRowsService rows,
        ISheetColumnBlocksService columnBlocks,
        ISheetCellSettingsService cellSettings,
        ISheetTypeDataService sheetTypes)
    {
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

    public async Task<SheetEditor> OpenSheetAsync(int sheetId, ShowcaseCellTypeCatalog cellTypes, CancellationToken cancellationToken)
    {
        var sheet = await _sheets.GetAsync(sheetId, SheetViewPoint.Live, cancellationToken);
        return new SheetEditor(sheet, cellTypes, _sheets, _tables, _sections, _rows, _columnBlocks, _cellSettings);
    }
}
