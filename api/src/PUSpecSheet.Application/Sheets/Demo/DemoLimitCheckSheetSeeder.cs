using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Application.Sheets.Demo;

/// <summary>
/// Adds a "Limits to check" table to the V6 and A3 Parts sheets, built from the sample "Limit checks"
/// template with the same services the API uses. Each row's first cell is a linked dropdown pointed at one
/// row of the sheet's "Piston parts" table, such as "Bore (mm)", so it offers every value in that row, with
/// one of them chosen. A table added by an earlier version of this sample, whose cells were pointed at the
/// column of descriptions, has them pointed at the matching rows instead.
/// <para>
/// What it does is published as the sheet's next version, unless the seeding user has unpublished changes
/// of their own on the sheet: a publish would take those with it, so the sample is then left as more
/// unpublished changes for them to publish. A sheet without the parts table is left alone, and so is one
/// whose "Limit checks" table has been removed.
/// </para>
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

    // Which of the sheet's limit rows (by what it is called) and who checks it.
    private static readonly (string Limit, string CheckedBy)[] Checks =
    [
        ("Bore (mm)", "Metrology"),
        ("Weight (g)", "Goods inwards"),
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

        var sheet = await sheets.GetAsync(sheetId.Value, SheetViewPoint.Live, cancellationToken);
        var parts = sheet.Tables.FirstOrDefault(table => table.TemplateName == DemoPartsGridTemplateSeeder.TemplateName);
        if (parts is null || parts.LinkableRows.Count == 0)
        {
            return;
        }

        var hadOwnChanges = sheet.MyDraftCount > 0;
        var existing = sheet.Tables.FirstOrDefault(table => table.TemplateName == DemoLimitCheckTemplateSeeder.TemplateName);
        bool changed;
        if (existing is not null)
        {
            changed = await PointAtRowsAsync(existing, parts, cancellationToken);
        }
        else if (await db.SheetTables.AnyAsync(table => table.SheetId == sheetId && table.TableTemplateVersion.TableTemplateId == templateId, cancellationToken))
        {
            // It was there once and has been removed: the sample is only ever added once.
            return;
        }
        else
        {
            await AddAsync(sheet.Id, templateId.Value, parts, cancellationToken);
            changed = true;
        }

        if (changed && !hadOwnChanges)
        {
            await sheets.PublishAsync(sheet.Id, new PublishSheetRequest("Sample limit checks pick from a row"), cancellationToken);
        }
    }

    private async Task AddAsync(int sheetId, int templateId, SheetTableDto parts, CancellationToken cancellationToken)
    {
        var dto = await tables.AddAsync(sheetId, new AddSheetTableRequest(templateId), cancellationToken);
        var tableId = Checked(dto).Id;
        try
        {
            await tables.SetTitleAsync(tableId, new UpdateSheetTableRequest("Limits to check"), cancellationToken);
            var checkTemplateId = Checked(dto).AddableSections.Single(section => section.Name == DemoLimitCheckTemplateSeeder.CheckName).TemplateSectionId;
            foreach (var (limit, checkedBy) in Checks)
            {
                dto = await sections.AddAsync(tableId, new AddSheetSectionRequest(checkTemplateId, null), cancellationToken);
                var row = Checked(dto).Sections[^1].Rows[0];
                await PointAtRowAsync(row, parts, limit, cancellationToken);
                await rows.SaveValuesAsync(
                    row.Id,
                    new SaveRowValuesRequest([new CellValueRequest(row.Cells[1].Id, checkedBy, null, null, null, null)]),
                    cancellationToken);
            }
        }
        catch
        {
            // Don't leave half a sample table behind. Only the table goes: the user's own changes stay.
            await tables.RemoveAsync(tableId, CancellationToken.None);
            throw;
        }
    }

    /// <summary>
    /// Points the cells of an existing sample table that still read the column of descriptions at the row
    /// each had picked. Cells someone has since pointed elsewhere, or emptied, are left as they are.
    /// </summary>
    private async Task<bool> PointAtRowsAsync(SheetTableDto table, SheetTableDto parts, CancellationToken cancellationToken)
    {
        var changed = false;
        foreach (var row in table.Sections.Where(section => section.Name == DemoLimitCheckTemplateSeeder.CheckName).SelectMany(section => section.Rows))
        {
            var cell = row.Cells[0];
            if (cell.Settings is LinkedDropdownInstanceSettings { SourceTemplateCellId: not null } && !string.IsNullOrWhiteSpace(cell.TextValue))
            {
                changed |= await PointAtRowAsync(row, parts, cell.TextValue, cancellationToken);
            }
        }

        return changed;
    }

    /// <summary>
    /// Points a row's first cell at the parts row called <paramref name="limit"/> and picks one of that
    /// row's values. Does nothing when the parts table has no such row.
    /// </summary>
    private async Task<bool> PointAtRowAsync(SheetRowDto row, SheetTableDto parts, string limit, CancellationToken cancellationToken)
    {
        var source = parts.LinkableRows.FirstOrDefault(candidate => candidate.Label == limit);
        if (source is null)
        {
            return false;
        }

        var settings = new LinkedDropdownInstanceSettings { SourceSheetTableId = parts.Id, SourceSheetRowId = source.SheetRowId };
        var dto = await cellSettings.SaveAsync(
            row.Id,
            new SaveRowCellSettingsRequest([new CellSettingsRequest(row.Cells[0].Id, settings)]),
            cancellationToken);
        var choices = dto.LinkedSources.FirstOrDefault(candidate => candidate.SheetRowId == source.SheetRowId)?.Options ?? [];
        if (choices.Count > 0)
        {
            // The second value is the first part's Max, which reads better as a limit than its Min.
            await rows.SaveValuesAsync(
                row.Id,
                new SaveRowValuesRequest([new CellValueRequest(row.Cells[0].Id, choices[Math.Min(1, choices.Count - 1)], null, null, null, null)]),
                cancellationToken);
        }

        return true;
    }

    private static SheetTableDto Checked(SheetDto dto)
    {
        return dto.Tables.Single(table => table.TemplateName == DemoLimitCheckTemplateSeeder.TemplateName);
    }
}
