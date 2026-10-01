using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Sheets.Demo;

/// <summary>
/// Fills in the V6 Specification sheet from the sample "Limits table" template using the same services
/// the API does, so the sample data is real published history: version 1 builds the table, version 2
/// tightens one limit. Does nothing if the sheet already has tables.
/// </summary>
public sealed class DemoLimitsSheetSeeder(
    PuSpecSheetDbContext db,
    ISheetService sheets,
    ISheetTableService tables,
    ISheetSectionService sections,
    ISheetRowService rows)
{
    private const string PhaseCode = "V6";

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var template = await db.TableTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Name == DemoLimitsTemplateSeeder.TemplateName, cancellationToken);
        var sheetTypeId = await db.SheetTypes
            .Where(candidate => candidate.Name == DemoLimitsTemplateSeeder.SheetTypeName)
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
        dto = await tables.SetTitleAsync(tableId, new UpdateSheetTableRequest("Valve limits"), cancellationToken);

        var groupTemplateId = Addable(dto, DemoLimitsTemplateSeeder.GroupName).TemplateSectionId;

        // First group: two limit rows and a single-value row.
        dto = await sections.AddAsync(tableId, new AddSheetSectionRequest(groupTemplateId, null), cancellationToken);
        var intake = LastTopLevelSection(dto);
        await SetTextAsync(intake.Rows[0], 0, "Intake valve", cancellationToken);

        var limitsTemplateId = AddableUnder(intake, DemoLimitsTemplateSeeder.LimitsName).TemplateSectionId;
        var singleTemplateId = AddableUnder(intake, DemoLimitsTemplateSeeder.SingleValueName).TemplateSectionId;

        dto = await sections.AddAsync(tableId, new AddSheetSectionRequest(limitsTemplateId, intake.Id), cancellationToken);
        await FillLimitsAsync(Child(dto, intake.Id, ^1), "Stem diameter (mm)", 6.97m, 6.98m, 6.99m, 7.00m, cancellationToken);

        dto = await sections.AddAsync(tableId, new AddSheetSectionRequest(limitsTemplateId, intake.Id), cancellationToken);
        await FillLimitsAsync(Child(dto, intake.Id, ^1), "Seat width (mm)", 1.20m, 1.40m, 1.60m, 1.80m, cancellationToken);

        dto = await sections.AddAsync(tableId, new AddSheetSectionRequest(singleTemplateId, intake.Id), cancellationToken);
        var note = Child(dto, intake.Id, ^1);
        await SetTextAsync(note.Rows[0], 0, "Finish", cancellationToken);
        await SetTextAsync(note.Rows[0], 1, "Lapped to the seat, no visible pitting", cancellationToken);

        // Second group: one limit row.
        dto = await sections.AddAsync(tableId, new AddSheetSectionRequest(groupTemplateId, null), cancellationToken);
        var exhaust = LastTopLevelSection(dto);
        await SetTextAsync(exhaust.Rows[0], 0, "Exhaust valve", cancellationToken);

        dto = await sections.AddAsync(tableId, new AddSheetSectionRequest(limitsTemplateId, exhaust.Id), cancellationToken);
        await FillLimitsAsync(Child(dto, exhaust.Id, ^1), "Stem diameter (mm)", 6.95m, 6.96m, 6.97m, 6.98m, cancellationToken);

        await sheets.PublishAsync(sheetId, new PublishSheetRequest("Sample limits"), cancellationToken);

        // Version 2: tighten the intake stem's lower limit, so there is some history to look at.
        dto = await sheets.GetAsync(sheetId, SheetViewPoint.Live, cancellationToken);
        var stem = Child(dto, FindGroup(dto, "Intake valve").Id, 0);
        await SetNumberAsync(stem.Rows[0], 1, 6.98m, cancellationToken);
        await sheets.PublishAsync(sheetId, new PublishSheetRequest("Tightened the intake stem lower limit"), cancellationToken);
    }

    private static AddableSectionDto Addable(SheetDto dto, string name)
    {
        return dto.Tables[0].AddableSections.Single(section => section.Name == name);
    }

    private static AddableSectionDto AddableUnder(SheetSectionDto section, string name)
    {
        return section.AddableSections.Single(candidate => candidate.Name == name);
    }

    private static SheetSectionDto LastTopLevelSection(SheetDto dto)
    {
        return dto.Tables[0].Sections[^1];
    }

    private static SheetSectionDto FindGroup(SheetDto dto, string description)
    {
        return dto.Tables[0].Sections
            .Single(section => section.Rows.Count > 0 && section.Rows[0].Cells.Count > 0 && section.Rows[0].Cells[0].TextValue == description);
    }

    private static SheetSectionDto Child(SheetDto dto, int parentSectionId, Index index)
    {
        var parent = dto.Tables[0].Sections.Single(section => section.Id == parentSectionId);
        return parent.Sections[index];
    }

    private async Task FillLimitsAsync(
        SheetSectionDto section,
        string description,
        decimal lower,
        decimal min,
        decimal max,
        decimal upper,
        CancellationToken cancellationToken)
    {
        var row = section.Rows[0];
        await rows.SaveValuesAsync(
            row.Id,
            new SaveRowValuesRequest(
            [
                new CellValueRequest(row.Cells[0].Id, description, null, null, null, null),
                new CellValueRequest(row.Cells[1].Id, null, lower, null, null, null),
                new CellValueRequest(row.Cells[2].Id, null, min, null, null, null),
                new CellValueRequest(row.Cells[3].Id, null, max, null, null, null),
                new CellValueRequest(row.Cells[4].Id, null, upper, null, null, null),
            ]),
            cancellationToken);
    }

    private async Task SetTextAsync(SheetRowDto row, int cellIndex, string value, CancellationToken cancellationToken)
    {
        await rows.SaveValuesAsync(
            row.Id,
            new SaveRowValuesRequest([new CellValueRequest(row.Cells[cellIndex].Id, value, null, null, null, null)]),
            cancellationToken);
    }

    private async Task SetNumberAsync(SheetRowDto row, int cellIndex, decimal value, CancellationToken cancellationToken)
    {
        await rows.SaveValuesAsync(
            row.Id,
            new SaveRowValuesRequest([new CellValueRequest(row.Cells[cellIndex].Id, null, value, null, null, null)]),
            cancellationToken);
    }
}
