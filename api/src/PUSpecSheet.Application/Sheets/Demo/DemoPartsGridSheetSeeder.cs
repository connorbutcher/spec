using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Sheets.Demo;

/// <summary>
/// Fills in the A3 Parts sheet from the sample "Parts limits" template using the same services the API does:
/// version 1 builds a table with a part number over each of four parts and a good spread of limit rows
/// across all of them, some a Min and Max and some a single value, and version 2 adds a fifth part, so the
/// new columns get every existing row. Does nothing if the sheet already has tables.
/// </summary>
public sealed class DemoPartsGridSheetSeeder(
    PuSpecSheetDbContext db,
    ISheetService sheets,
    ISheetTableService tables,
    ISheetSectionService sections,
    ISheetColumnBlockService columnBlocks,
    ISheetRowService rows)
{
    private const string PhaseCode = "A3";

    private const int StartingParts = 4;

    private static readonly string[] PartNumbers = ["P-1001", "P-1002", "P-1003", "P-1004", "P-1005"];

    // Description, whether it's one value (rather than Min and Max), the value for the first part, how much
    // it differs from one part to the next, and the allowed spread either side for a Min and Max.
    private static readonly (string Description, bool Single, decimal Base, decimal Step, decimal Spread)[] Rows =
    [
        ("Bore (mm)", false, 82.00m, 0.01m, 0.02m),
        ("Stroke (mm)", false, 94.60m, 0.00m, 0.02m),
        ("Material hardness (HV)", true, 118m, 2m, 0m),
        ("Pin bore (mm)", false, 21.00m, 0.00m, 0.01m),
        ("Compression height (mm)", false, 31.50m, 0.05m, 0.03m),
        ("Crown volume (cc)", true, 6.40m, 0.10m, 0m),
        ("Ring groove 1 width (mm)", false, 1.20m, 0.00m, 0.02m),
        ("Ring groove 2 width (mm)", false, 1.20m, 0.00m, 0.02m),
        ("Oil ring groove width (mm)", false, 2.00m, 0.00m, 0.02m),
        ("Skirt diameter (mm)", false, 81.95m, 0.01m, 0.02m),
        ("Surface roughness Ra (um)", true, 0.40m, 0.02m, 0m),
        ("Weight (g)", false, 410m, 3m, 5m),
        ("Pin offset (mm)", true, 0.80m, 0.00m, 0m),
        ("Maximum operating temperature (C)", true, 320m, 5m, 0m),
        ("Ring gap, top ring (mm)", false, 0.30m, 0.01m, 0.05m),
        ("Ring gap, second ring (mm)", false, 0.45m, 0.01m, 0.05m),
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

        var limitsTemplateId = dto.Tables[0].AddableSections.Single(section => section.Name == DemoPartsGridTemplateSeeder.LimitsName).TemplateSectionId;
        var singleTemplateId = dto.Tables[0].AddableSections.Single(section => section.Name == DemoPartsGridTemplateSeeder.SingleValueName).TemplateSectionId;

        foreach (var spec in Rows)
        {
            var templateSectionId = spec.Single ? singleTemplateId : limitsTemplateId;
            dto = await sections.AddAsync(tableId, new AddSheetSectionRequest(templateSectionId, null), cancellationToken);
            var row = dto.Tables[0].Sections[^1].Rows[0];
            var requests = new List<CellValueRequest> { new(StubCell(row).Id, spec.Description, null, null, null, null) };
            requests.AddRange(ValueRequests(dto.Tables[0], row, spec, from: 0, count: StartingParts));
            await rows.SaveValuesAsync(row.Id, new SaveRowValuesRequest(requests), cancellationToken);
        }

        await SavePartNumbersAsync(dto.Tables[0], from: 0, count: StartingParts, cancellationToken);
        await sheets.PublishAsync(sheetId, new PublishSheetRequest("Sample parts"), cancellationToken);

        // Version 2: a fifth part. Every existing row gets its cells in the new columns.
        var partTemplateId = dto.Tables[0].AddableColumnBlocks.Single().TemplateColumnBlockId;
        dto = await columnBlocks.AddAsync(tableId, new AddSheetColumnBlockRequest(partTemplateId), cancellationToken);
        var table = dto.Tables[0];
        var specSections = table.Sections.Where(section => section.Name != "Header").ToList();
        for (var index = 0; index < specSections.Count; index++)
        {
            var row = specSections[index].Rows[0];
            var requests = ValueRequests(table, row, Rows[index], from: StartingParts, count: 1);
            await rows.SaveValuesAsync(row.Id, new SaveRowValuesRequest(requests), cancellationToken);
        }

        await SavePartNumbersAsync(table, from: StartingParts, count: 1, cancellationToken);
        await sheets.PublishAsync(sheetId, new PublishSheetRequest("Added a fifth part"), cancellationToken);
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

    /// <summary>
    /// The values of one row for parts <paramref name="from"/> onwards: a Min and a Max in each part's two
    /// cells, or a single value in its one cell.
    /// </summary>
    private static List<CellValueRequest> ValueRequests(
        SheetTableDto table,
        SheetRowDto row,
        (string Description, bool Single, decimal Base, decimal Step, decimal Spread) spec,
        int from,
        int count)
    {
        var requests = new List<CellValueRequest>();
        for (var part = from; part < from + count; part++)
        {
            var centre = Math.Round(spec.Base + (spec.Step * part), 2);
            var cells = BlockCells(row, table.ColumnBlocks[part].Id);
            if (spec.Single)
            {
                requests.Add(new CellValueRequest(cells[0].Id, null, centre, null, null, null));
                continue;
            }

            requests.Add(new CellValueRequest(cells[0].Id, null, centre - spec.Spread, null, null, null));
            requests.Add(new CellValueRequest(cells[1].Id, null, centre + spec.Spread, null, null, null));
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
