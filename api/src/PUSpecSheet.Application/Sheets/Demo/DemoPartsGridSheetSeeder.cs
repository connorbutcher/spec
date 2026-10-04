using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Sheets.Demo;

/// <summary>
/// Fills in the V6 Parts sheet from the sample "Parts grid" template using the same services the API does:
/// version 1 builds a table with three spec rows across the two starting parts, and version 2 adds a third
/// part, so the new column has every existing row. Does nothing if the sheet already has tables.
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

    private static readonly (string Description, decimal[] Values)[] Specs =
    [
        ("Bore (mm)", [82.00m, 82.01m]),
        ("Stroke (mm)", [94.60m, 94.60m]),
        ("Weight (g)", [412m, 409m]),
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

        foreach (var (description, values) in Specs)
        {
            dto = await sections.AddAsync(tableId, new AddSheetSectionRequest(specTemplateId, null), cancellationToken);
            var row = dto.Tables[0].Sections[^1].Rows[0];
            await SaveRowAsync(row, description, values, cancellationToken);
        }

        await sheets.PublishAsync(sheetId, new PublishSheetRequest("Sample parts"), cancellationToken);

        // Version 2: another part. Every existing row gets its cell in the new column.
        var partTemplateId = dto.Tables[0].AddableColumnBlocks.Single().TemplateColumnBlockId;
        dto = await columnBlocks.AddAsync(tableId, new AddSheetColumnBlockRequest(partTemplateId), cancellationToken);
        var extraValues = new[] { 82.02m, 94.61m, 415m };
        var specRows = dto.Tables[0].Sections.Where(section => section.Rows.Count > 0 && section.Name == DemoPartsGridTemplateSeeder.SpecName).ToList();
        for (var index = 0; index < specRows.Count; index++)
        {
            var row = specRows[index].Rows[0];
            var cell = row.Cells.Last(candidate => candidate.SheetColumnBlockId is not null);
            await rows.SaveValuesAsync(
                row.Id,
                new SaveRowValuesRequest([new CellValueRequest(cell.Id, null, extraValues[index], null, null, null)]),
                cancellationToken);
        }

        await sheets.PublishAsync(sheetId, new PublishSheetRequest("Added a third part"), cancellationToken);
    }

    private async Task SaveRowAsync(SheetRowDto row, string description, decimal[] values, CancellationToken cancellationToken)
    {
        var stub = row.Cells.First(cell => cell.SheetColumnBlockId is null);
        var blockCells = row.Cells.Where(cell => cell.SheetColumnBlockId is not null).ToList();

        var requests = new List<CellValueRequest> { new(stub.Id, description, null, null, null, null) };
        for (var index = 0; index < blockCells.Count && index < values.Length; index++)
        {
            requests.Add(new CellValueRequest(blockCells[index].Id, null, values[index], null, null, null));
        }

        await rows.SaveValuesAsync(row.Id, new SaveRowValuesRequest(requests), cancellationToken);
    }
}
