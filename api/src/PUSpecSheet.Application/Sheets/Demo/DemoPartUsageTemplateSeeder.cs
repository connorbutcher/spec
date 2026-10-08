using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets.Demo;

/// <summary>
/// Adds the "Part usage" sample template to the Parts sheet type: a vertical table of rows people add, each
/// a part picked from a linked dropdown, where it is fitted and how many. Which table the dropdown reads is
/// chosen on the sheet, so the template says nothing about it. Also adds a "Linked dropdown" cell type when
/// there is none of that kind. Does nothing if a template with that name already exists.
/// </summary>
public sealed class DemoPartUsageTemplateSeeder(PuSpecSheetDbContext db)
{
    public const string TemplateName = "Part usage";
    public const string UsageName = "Usage";
    public const string LinkedCellTypeName = "Linked dropdown";

    public async Task<bool> SeedAsync(CancellationToken cancellationToken)
    {
        var sheetType = await db.SheetTypes.SingleOrDefaultAsync(candidate => candidate.Name == DemoPartsGridTemplateSeeder.SheetTypeName, cancellationToken);
        if (sheetType is null)
        {
            return false;
        }

        if (await db.TableTemplates.AnyAsync(template => template.SheetTypeId == sheetType.Id && template.Name == TemplateName, cancellationToken))
        {
            return false;
        }

        var heading = await FindCellTypeAsync(CellKind.Heading, cancellationToken);
        var text = await FindCellTypeAsync(CellKind.Text, cancellationToken);
        var number = await FindCellTypeAsync(CellKind.Number, cancellationToken);
        if (heading is null || text is null || number is null)
        {
            return false;
        }

        var linked = await FindCellTypeAsync(CellKind.LinkedDropdown, cancellationToken) ?? await AddLinkedCellTypeAsync(cancellationToken);
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

        var header = NewSection(version, "Header", SectionRole.Header, 1, minimum: 1, maximum: 1, initial: 1);
        header.Rows.Add(NewRow(Cell(heading.Id, 1, "Part"), Cell(heading.Id, 2, "Fitted to"), Cell(heading.Id, 3, "Quantity")));

        var usage = NewSection(version, UsageName, SectionRole.Addable, 2, minimum: 0, maximum: null, initial: 0);
        var quantity = Cell(number.Id, 3, null);
        quantity.ConfigurationOverride = new NumberCellConfiguration { DecimalPlaces = 0 };
        usage.Rows.Add(NewRow(Cell(linked.Id, 1, null), Cell(text.Id, 2, null), quantity));

        db.TableTemplates.Add(template);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<CellType> AddLinkedCellTypeAsync(CancellationToken cancellationToken)
    {
        var nameTaken = await db.CellTypes.AnyAsync(cellType => cellType.Name == LinkedCellTypeName, cancellationToken);
        var order = await db.CellTypes.MaxAsync(cellType => (int?)cellType.DisplayOrder, cancellationToken) ?? 0;
        var cellType = new CellType
        {
            Name = nameTaken ? $"{LinkedCellTypeName} (sample)" : LinkedCellTypeName,
            Kind = CellKind.LinkedDropdown,
            Description = "A choice from a column of another table, picked on the sheet.",
            DisplayOrder = order + 1,
            Configuration = new LinkedDropdownCellConfiguration(),
        };
        db.CellTypes.Add(cellType);
        await db.SaveChangesAsync(cancellationToken);
        return cellType;
    }

    private async Task<CellType?> FindCellTypeAsync(CellKind kind, CancellationToken cancellationToken)
    {
        return await db.CellTypes
            .AsNoTracking()
            .Where(cellType => cellType.Kind == kind)
            .OrderBy(cellType => cellType.DisplayOrder)
            .ThenBy(cellType => cellType.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static TemplateSection NewSection(
        TableTemplateVersion version,
        string name,
        SectionRole role,
        int displayOrder,
        int minimum,
        int? maximum,
        int initial)
    {
        var section = new TemplateSection
        {
            Name = name,
            Role = role,
            DisplayOrder = displayOrder,
            MinInstances = minimum,
            MaxInstances = maximum,
            InitialInstances = initial,
        };
        version.Sections.Add(section);
        return section;
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

    private static TemplateCell Cell(int cellTypeId, int column, string? caption)
    {
        return new TemplateCell { CellTypeId = cellTypeId, Column = column, Caption = caption };
    }
}
