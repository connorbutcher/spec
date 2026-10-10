using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets.Demo;

/// <summary>
/// Adds the "Limit checks" sample template to the Parts sheet type: a vertical table of rows people add,
/// each a limit picked from a linked dropdown and who checks it. On the sheet the dropdown is pointed at a
/// column of descriptions and set to give the picked row's other values too. It uses the linked dropdown
/// cell type the "Part usage" sample adds, and does nothing without it or if the template already exists.
/// </summary>
public sealed class DemoLimitCheckTemplateSeeder(PuSpecSheetDbContext db)
{
    public const string TemplateName = "Limit checks";
    public const string CheckName = "Check";

    public async Task<bool> SeedAsync(CancellationToken cancellationToken)
    {
        var sheetType = await db.SheetTypes.SingleOrDefaultAsync(candidate => candidate.Name == DemoPartsGridTemplateSeeder.SheetTypeName, cancellationToken);
        if (sheetType is null
            || await db.TableTemplates.AnyAsync(template => template.SheetTypeId == sheetType.Id && template.Name == TemplateName, cancellationToken))
        {
            return false;
        }

        var heading = await FindCellTypeIdAsync(CellKind.Heading, cancellationToken);
        var text = await FindCellTypeIdAsync(CellKind.Text, cancellationToken);
        var linked = await FindCellTypeIdAsync(CellKind.LinkedDropdown, cancellationToken);
        if (heading is null || text is null || linked is null)
        {
            return false;
        }

        var order = await db.TableTemplates
            .Where(template => template.SheetTypeId == sheetType.Id)
            .MaxAsync(template => (int?)template.DisplayOrder, cancellationToken) ?? 0;

        var template = new TableTemplate { SheetTypeId = sheetType.Id, Name = TemplateName, DisplayOrder = order + 1 };
        var version = new TableTemplateVersion
        {
            VersionNumber = 1,
            Orientation = TemplateOrientation.Vertical,
            CreatedAtUtc = DateTime.UtcNow,
        };
        template.Versions.Add(version);

        var header = new TemplateSection { Name = "Header", Role = SectionRole.Header, DisplayOrder = 1, MinInstances = 1, MaxInstances = 1, InitialInstances = 1 };
        header.Rows.Add(NewRow(Cell(heading.Value, 1, 2, "Limit"), Cell(heading.Value, 3, 1, "Checked by")));
        version.Sections.Add(header);

        var check = new TemplateSection { Name = CheckName, Role = SectionRole.Addable, DisplayOrder = 2 };
        check.Rows.Add(NewRow(Cell(linked.Value, 1, 2, null), Cell(text.Value, 3, 1, null)));
        version.Sections.Add(check);

        db.TableTemplates.Add(template);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<int?> FindCellTypeIdAsync(CellKind kind, CancellationToken cancellationToken)
    {
        return await db.CellTypes
            .Where(cellType => cellType.Kind == kind)
            .OrderBy(cellType => cellType.DisplayOrder)
            .ThenBy(cellType => cellType.Id)
            .Select(cellType => (int?)cellType.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static TemplateRow NewRow(params TemplateCell[] cells)
    {
        var row = new TemplateRow { DisplayOrder = 1 };
        foreach (var cell in cells)
        {
            row.Cells.Add(cell);
        }

        return row;
    }

    private static TemplateCell Cell(int cellTypeId, int column, int columnSpan, string? caption)
    {
        return new TemplateCell { CellTypeId = cellTypeId, Column = column, ColumnSpan = columnSpan, Caption = caption };
    }
}
