using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Seeding;

/// <summary>Opens a phase's sheet of a sheet type, creating it as the app does the first time, for a <see cref="SheetEditor"/>.</summary>
public sealed class SheetEditorFactory(
    PuSpecSheetDbContext db,
    ISheetService sheets,
    ISheetTableService tables,
    ISheetSectionService sections,
    ISheetRowService rows,
    ISheetColumnBlockService columnBlocks,
    ISheetCellSettingsService cellSettings)
{
    public async Task<SheetEditor> OpenAsync(int phaseId, string sheetTypeName, ShowcaseCellTypeCatalog cellTypes, CancellationToken cancellationToken)
    {
        var sheetTypeId = await db.SheetTypes
            .Where(sheetType => sheetType.Name == sheetTypeName)
            .Select(sheetType => sheetType.Id)
            .SingleAsync(cancellationToken);
        var sheet = await sheets.OpenAsync(phaseId, sheetTypeId, SheetViewPoint.Live, cancellationToken);
        return new SheetEditor(sheet, cellTypes, sheets, tables, sections, rows, columnBlocks, cellSettings);
    }
}
