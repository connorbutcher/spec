using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Application.Sheets.Demo;

/// <summary>
/// Adds a "Limits to check" table to the V6 and A3 Parts sheets, built from the sample "Limit checks"
/// template with the same services the API uses. Each row's first cell is a linked dropdown pointed at the
/// descriptions of the sheet's "Piston parts" limits and set to give the picked row's other values, so
/// picking "Bore (mm)" shows every part's Min and Max beside it. The result is published as the sheet's
/// next version, unless the seeding user has unpublished changes of their own on the sheet: a publish would
/// take those with it, so the table is then left as one more unpublished change for them to publish. A
/// sheet without the parts table, and one that has ever had a "Limit checks" table, are left alone.
/// </summary>
public sealed class DemoLimitCheckSheetSeeder(
    PuSpecSheetDbContext db,
    ISheetService sheets,
    ISheetTableService tables,
    ISheetSectionService sections,
    ISheetRowService rows,
    ISheetCellSettingsService cellSettings)
{
    private static readonly string[] PhaseCodes = ["V6", "A3"];

    // Which of the sheet's limits (counted from the first) and who checks it.
    private static readonly (int Limit, string CheckedBy)[] Checks =
    [
        (0, "Metrology"),
        (8, "Goods inwards"),
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
            .Where(candidate => candidate.Name == DemoLimitCheckTemplateSeeder.TemplateName
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
        var limits = parts?.Sections.Where(section => section.Name == DemoPartsGridTemplateSeeder.LimitsName).ToList() ?? [];
        if (parts is null || limits.Count == 0)
        {
            return;
        }

        // The description is the limit rows' own first cell; its column is found from one of them.
        var descriptions = limits
            .Select(section => section.Rows[0].Cells.First(cell => cell.SheetColumnBlockId is null))
            .Where(cell => !string.IsNullOrWhiteSpace(cell.TextValue))
            .ToList();
        if (descriptions.Count == 0 || parts.LinkableColumns.All(column => column.TemplateCellId != descriptions[0].Template.Id))
        {
            return;
        }

        var source = new LinkedDropdownInstanceSettings
        {
            SourceSheetTableId = parts.Id,
            SourceTemplateCellId = descriptions[0].Template.Id,
            IncludeRowValues = true,
        };
        var dto = await tables.AddAsync(sheet.Id, new AddSheetTableRequest(templateId.Value), cancellationToken);
        var tableId = Checked(dto).Id;
        try
        {
            await BuildAsync(dto, source, descriptions.Select(cell => cell.TextValue!).ToList(), cancellationToken);
            if (sheet.MyDraftCount == 0)
            {
                await sheets.PublishAsync(sheet.Id, new PublishSheetRequest("Sample limit checks"), cancellationToken);
            }
        }
        catch
        {
            // Don't leave half a sample table behind. Only the table goes: the user's own changes stay.
            await tables.RemoveAsync(tableId, CancellationToken.None);
            throw;
        }
    }

    private async Task BuildAsync(
        SheetDto dto,
        LinkedDropdownInstanceSettings source,
        List<string> limits,
        CancellationToken cancellationToken)
    {
        var tableId = Checked(dto).Id;
        await tables.SetTitleAsync(tableId, new UpdateSheetTableRequest("Limits to check"), cancellationToken);
        var checkTemplateId = Checked(dto).AddableSections.Single(section => section.Name == DemoLimitCheckTemplateSeeder.CheckName).TemplateSectionId;

        foreach (var (limit, checkedBy) in Checks)
        {
            dto = await sections.AddAsync(tableId, new AddSheetSectionRequest(checkTemplateId, null), cancellationToken);
            var row = Checked(dto).Sections[^1].Rows[0];

            // The first row is pointed at the descriptions; the rows added after it start with the same choice.
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
                    new CellValueRequest(row.Cells[0].Id, limits[Math.Min(limit, limits.Count - 1)], null, null, null, null),
                    new CellValueRequest(row.Cells[1].Id, checkedBy, null, null, null, null),
                ]),
                cancellationToken);
        }
    }

    private static SheetTableDto Checked(SheetDto dto)
    {
        return dto.Tables.Single(table => table.TemplateName == DemoLimitCheckTemplateSeeder.TemplateName);
    }
}
