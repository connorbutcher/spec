using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Sheets.Demo;

/// <summary>
/// Fills in the V6 Parts sheet from the sample "Parts grid" template using the same services the API does:
/// version 1 builds a table with a part number over each of two parts and three rows of limits across both,
/// and version 2 adds a third part, so the new columns get every existing row. Does nothing if the sheet
/// already has tables.
/// </summary>
public sealed class DemoPartsGridSheetSeeder(
    PuSpecSheetDbContext db,
    ISheetService sheets,
    ISheetTableService tables,
    ISheetSectionService sections,
    ISheetColumnBlockService columnBlocks,
    ISheetRowService rows)
{
    private const string PhaseCode = "V6";

    private static readonly string[] PartNumbers = ["P-1001", "P-1002", "P-1003"];

    // Description, then Min and Max for each part in turn.
    private static readonly (string Description, decimal[][] Limits)[] Specs =
    [
        ("Bore (mm)", [[81.98m, 82.02m], [81.99m, 82.03m], [82.00m, 82.04m]]),
        ("Stroke (mm)", [[94.58m, 94.62m], [94.58m, 94.62m], [94.59m, 94.63m]]),
        ("Weight (g)", [[405m, 415m], [402m, 412m], [408m, 418m]]),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var template = await db.TableTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Name == DemoPartsGridTemplateSeeder.TemplateName, cancellationToken);
        var sheetTypeId = await db.SheetTypes
            .Where(candidate => candidate.Name == DemoPartsGridTemplateSeeder.SheetTypeName)
            .Select(candidate => (int?)candidate.Id)
            .SingleOrDefaultAsync(cancellationToken);
        var phaseId = await db.Phases
            .Where(candidate => candidate.Code == PhaseCode)
            .Select(candidate => (int?)candidate.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (template is null || sheetTypeId is null || phaseId is null)
        {
            return;
        }

        var sheet = await sheets.OpenAsync(phaseId.Value, sheetTypeId.Value, SheetViewPoint.Live, cancellationToken);
        if (sheet.Tables.Count > 0 || sheet.Versions.Count > 0)
        {
            return;
        }

        try
        {
            await BuildAsync(sheet.Id, template.Id, cancellationToken);
        }
        catch
        {
            // Don't leave half a sample sheet behind as drafts that would stop it being seeded again.
            await sheets.DiscardDraftsAsync(sheet.Id, CancellationToken.None);
            throw;
        }
    }

    private async Task BuildAsync(int sheetId, int templateId, CancellationToken cancellationToken)
    {
        var dto = await tables.AddAsync(sheetId, new AddSheetTableRequest(templateId), cancellationToken);
        var tableId = dto.Tables[0].Id;
        await tables.SetTitleAsync(tableId, new UpdateSheetTableRequest("Piston parts"), cancellationToken);

        var specTemplateId = dto.Tables[0].AddableSections.Single(section => section.Name == DemoPartsGridTemplateSeeder.SpecName).TemplateSectionId;

        foreach (var (description, limits) in Specs)
        {
            dto = await sections.AddAsync(tableId, new AddSheetSectionRequest(specTemplateId, null), cancellationToken);
            var row = dto.Tables[0].Sections[^1].Rows[0];
            var requests = new List<CellValueRequest> { new(StubCell(row).Id, description, null, null, null, null) };
            requests.AddRange(LimitRequests(dto.Tables[0], row, limits, from: 0, count: 2));
            await rows.SaveValuesAsync(row.Id, new SaveRowValuesRequest(requests), cancellationToken);
        }

        await SavePartNumbersAsync(dto.Tables[0], from: 0, count: 2, cancellationToken);
        await sheets.PublishAsync(sheetId, new PublishSheetRequest("Sample parts"), cancellationToken);

        // Version 2: a third part. Every existing row gets its Min and Max cells in the new columns.
        var partTemplateId = dto.Tables[0].AddableColumnBlocks.Single().TemplateColumnBlockId;
        dto = await columnBlocks.AddAsync(tableId, new AddSheetColumnBlockRequest(partTemplateId), cancellationToken);
        var table = dto.Tables[0];
        var specRows = table.Sections.Where(section => section.Name == DemoPartsGridTemplateSeeder.SpecName).Select(section => section.Rows[0]).ToList();
        for (var index = 0; index < specRows.Count; index++)
        {
            var requests = LimitRequests(table, specRows[index], Specs[index].Limits, from: 2, count: 1);
            await rows.SaveValuesAsync(specRows[index].Id, new SaveRowValuesRequest(requests), cancellationToken);
        }

        await SavePartNumbersAsync(table, from: 2, count: 1, cancellationToken);
        await sheets.PublishAsync(sheetId, new PublishSheetRequest("Added a third part"), cancellationToken);
    }

    /// <summary>The part number of each part in the header's first row, for <paramref name="count"/> parts from <paramref name="from"/>.</summary>
    private async Task SavePartNumbersAsync(SheetTableDto table, int from, int count, CancellationToken cancellationToken)
    {
        var header = table.Sections[0].Rows[0];
        var requests = new List<CellValueRequest>();
        for (var index = from; index < from + count; index++)
        {
            var partNumber = BlockCells(header, table.ColumnBlocks[index].Id).First();
            requests.Add(new CellValueRequest(partNumber.Id, PartNumbers[index], null, null, null, null));
        }

        await rows.SaveValuesAsync(header.Id, new SaveRowValuesRequest(requests), cancellationToken);
    }

    /// <summary>Min and Max for parts <paramref name="from"/> onwards, written into each part's own two cells.</summary>
    private static List<CellValueRequest> LimitRequests(SheetTableDto table, SheetRowDto row, decimal[][] limits, int from, int count)
    {
        var requests = new List<CellValueRequest>();
        for (var part = from; part < from + count; part++)
        {
            var cells = BlockCells(row, table.ColumnBlocks[part].Id);
            requests.Add(new CellValueRequest(cells[0].Id, null, limits[part][0], null, null, null));
            requests.Add(new CellValueRequest(cells[1].Id, null, limits[part][1], null, null, null));
        }

        return requests;
    }

    private static SheetCellDto StubCell(SheetRowDto row)
    {
        return row.Cells.First(cell => cell.SheetColumnBlockId is null);
    }

    /// <summary>A row's cells in one column block copy, left to right.</summary>
    private static List<SheetCellDto> BlockCells(SheetRowDto row, int columnBlockId)
    {
        return row.Cells
            .Where(cell => cell.SheetColumnBlockId == columnBlockId)
            .OrderBy(cell => cell.Template.Column)
            .ToList();
    }
}
