using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Application.Sheets.Demo;

/// <summary>
/// Adds a "Where parts are used" table to the V6 and A3 Parts sheets, built from the sample "Part usage"
/// template with the same services the API uses. Each row's first cell is a linked dropdown pointed at the
/// part numbers of the sheet's "Piston parts" table, with one of them chosen, and the result is published
/// as the sheet's next version. A sheet without the parts table, one that has ever had a "Part usage" table,
/// and one where the seeding user has unpublished changes (which the publish would take with it) are left alone.
/// </summary>
public sealed class DemoPartUsageSheetSeeder(
    PuSpecSheetDbContext db,
    ISheetService sheets,
    ISheetTableService tables,
    ISheetSectionService sections,
    ISheetRowService rows,
    ISheetCellSettingsService cellSettings)
{
    private static readonly string[] PhaseCodes = ["V6", "A3"];

    // Which of the sheet's parts (counted from the first), where it is fitted and how many.
    private static readonly (int Part, string FittedTo, decimal Quantity)[] Usages =
    [
        (0, "Cylinders 1 and 4", 2m),
        (2, "Cylinders 2 and 3", 2m),
        (4, "Service kit", 6m),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        foreach (var phaseCode in PhaseCodes)
        {
            await SeedPhaseAsync(phaseCode, cancellationToken);
        }
    }

    private async Task SeedPhaseAsync(string phaseCode, CancellationToken cancellationToken)
    {
        var templateId = await db.TableTemplates
            .Where(candidate => candidate.Name == DemoPartUsageTemplateSeeder.TemplateName
                && candidate.SheetType.Name == DemoPartsGridTemplateSeeder.SheetTypeName)
            .Select(candidate => (int?)candidate.Id)
            .SingleOrDefaultAsync(cancellationToken);
        var sheetId = await db.Sheets
            .Where(candidate => candidate.Phase.Code == phaseCode
                && candidate.SheetType.Name == DemoPartsGridTemplateSeeder.SheetTypeName)
            .Select(candidate => (int?)candidate.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (templateId is null || sheetId is null)
        {
            return;
        }

        // Even a table that was later removed counts: the sample is only ever added once.
        if (await db.SheetTables.AnyAsync(table => table.SheetId == sheetId && table.TableTemplateVersion.TableTemplateId == templateId, cancellationToken))
        {
            return;
        }

        var sheet = await sheets.GetAsync(sheetId.Value, SheetViewPoint.Live, cancellationToken);
        var parts = sheet.Tables.FirstOrDefault(table => table.TemplateName == DemoPartsGridTemplateSeeder.TemplateName);
        var partNumberColumn = parts?.LinkableColumns.FirstOrDefault(column => column.Label == DemoPartsGridTemplateSeeder.PartNumberCaption);
        if (parts is null || partNumberColumn is null || sheet.MyDraftCount > 0)
        {
            return;
        }

        var partNumbers = parts.Sections
            .SelectMany(section => section.Rows)
            .SelectMany(row => row.Cells)
            .Where(cell => cell.Template.Id == partNumberColumn.TemplateCellId && !string.IsNullOrWhiteSpace(cell.TextValue))
            .Select(cell => cell.TextValue!)
            .ToList();
        if (partNumbers.Count == 0)
        {
            return;
        }

        try
        {
            var source = new LinkedDropdownInstanceSettings { SourceSheetTableId = parts.Id, SourceTemplateCellId = partNumberColumn.TemplateCellId };
            await BuildAsync(sheet.Id, templateId.Value, source, partNumbers, cancellationToken);
        }
        catch
        {
            // Don't leave half a sample table behind as drafts. The user had none of their own on the sheet.
            await sheets.DiscardDraftsAsync(sheet.Id, CancellationToken.None);
            throw;
        }
    }

    private async Task BuildAsync(
        int sheetId,
        int templateId,
        LinkedDropdownInstanceSettings source,
        List<string> partNumbers,
        CancellationToken cancellationToken)
    {
        var dto = await tables.AddAsync(sheetId, new AddSheetTableRequest(templateId), cancellationToken);
        var tableId = Usage(dto).Id;
        await tables.SetTitleAsync(tableId, new UpdateSheetTableRequest("Where parts are used"), cancellationToken);
        var usageTemplateId = Usage(dto).AddableSections.Single(section => section.Name == DemoPartUsageTemplateSeeder.UsageName).TemplateSectionId;

        foreach (var (part, fittedTo, quantity) in Usages)
        {
            dto = await sections.AddAsync(tableId, new AddSheetSectionRequest(usageTemplateId, null), cancellationToken);
            var row = Usage(dto).Sections[^1].Rows[0];

            // The first row is pointed at the part numbers; the rows added after it start with the same choice.
            if (row.Cells[0].Settings is null)
            {
                await cellSettings.SaveAsync(
                    row.Id,
                    new SaveRowCellSettingsRequest([new CellSettingsRequest(row.Cells[0].Id, source)]),
                    cancellationToken);
            }

            await rows.SaveValuesAsync(
                row.Id,
                new SaveRowValuesRequest(
                [
                    new CellValueRequest(row.Cells[0].Id, partNumbers[Math.Min(part, partNumbers.Count - 1)], null, null, null, null),
                    new CellValueRequest(row.Cells[1].Id, fittedTo, null, null, null, null),
                    new CellValueRequest(row.Cells[2].Id, null, quantity, null, null, null),
                ]),
                cancellationToken);
        }

        await sheets.PublishAsync(sheetId, new PublishSheetRequest("Sample part usage"), cancellationToken);
    }

    private static SheetTableDto Usage(SheetDto dto)
    {
        return dto.Tables.Single(table => table.TemplateName == DemoPartUsageTemplateSeeder.TemplateName);
    }
}
