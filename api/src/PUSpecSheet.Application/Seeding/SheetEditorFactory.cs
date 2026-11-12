using PUSpecSheet.Application.Persistence.SheetTypes;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Application.Sheets.Editing;
using PUSpecSheet.Application.Sheets.Reading;

namespace PUSpecSheet.Application.Seeding;

/// <summary>Opens a phase's sheet of a sheet type, creating it as the app does the first time, for a <see cref="SheetEditor"/>.</summary>
public sealed class SheetEditorFactory
{
    private readonly ISheetTypeRepository sheetTypes;
    private readonly ISheetService sheets;
    private readonly ISheetTableService tables;
    private readonly ISheetSectionService sections;
    private readonly ISheetRowService rows;
    private readonly ISheetColumnBlockService columnBlocks;
    private readonly ISheetCellSettingsService cellSettings;

    public SheetEditorFactory(
        ISheetTypeRepository sheetTypes,
        ISheetService sheets,
        ISheetTableService tables,
        ISheetSectionService sections,
        ISheetRowService rows,
        ISheetColumnBlockService columnBlocks,
        ISheetCellSettingsService cellSettings)
    {
        this.sheetTypes = sheetTypes;
        this.sheets = sheets;
        this.tables = tables;
        this.sections = sections;
        this.rows = rows;
        this.columnBlocks = columnBlocks;
        this.cellSettings = cellSettings;
    }

    public async Task<SheetEditor> OpenAsync(int phaseId, string sheetTypeName, ShowcaseCellTypeCatalog cellTypes, CancellationToken cancellationToken)
    {
        var sheetTypeId = await sheetTypes.GetIdByNameAsync(sheetTypeName, cancellationToken);
        var sheet = await sheets.OpenAsync(phaseId, sheetTypeId, SheetViewPoint.Live, cancellationToken);
        return new SheetEditor(sheet, cellTypes, sheets, tables, sections, rows, columnBlocks, cellSettings);
    }
}
