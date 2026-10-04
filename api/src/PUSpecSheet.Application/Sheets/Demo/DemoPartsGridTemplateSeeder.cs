using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets.Demo;

/// <summary>
/// Adds the "Parts grid" sample template to the Parts sheet type: a horizontal table whose own column holds
/// the descriptions and whose "Part" column block people add copies of, one per part. The header row names
/// the columns and the addable "Spec" section has one row, a description and a value for each part. Does
/// nothing if a template with that name already exists.
/// </summary>
public sealed class DemoPartsGridTemplateSeeder(PuSpecSheetDbContext db)
{
    public const string SheetTypeName = "Parts";
    public const string TemplateName = "Parts grid";
    public const string SpecName = "Spec";
    public const string PartBlockName = "Part";

    public async Task<bool> SeedAsync(CancellationToken cancellationToken)
    {
        var sheetType = await db.SheetTypes.SingleOrDefaultAsync(candidate => candidate.Name == SheetTypeName, cancellationToken);
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

        var order = await db.TableTemplates
            .Where(template => template.SheetTypeId == sheetType.Id)
            .MaxAsync(template => (int?)template.DisplayOrder, cancellationToken) ?? 0;

        var template = new TableTemplate { SheetTypeId = sheetType.Id, Name = TemplateName, DisplayOrder = order + 1 };
        var version = new TableTemplateVersion
        {
            VersionNumber = 1,
            Orientation = TemplateOrientation.Horizontal,
            CreatedAtUtc = DateTime.UtcNow,
        };
        template.Versions.Add(version);

        var part = new TemplateColumnBlock
        {
            Name = PartBlockName,
            DisplayOrder = 1,
            MinInstances = 0,
            MaxInstances = null,
            InitialInstances = 2,
        };
        version.ColumnBlocks.Add(part);

        var header = NewSection(version, "Header", SectionRole.Header, 1, minimum: 1, maximum: 1, initial: 1);
        header.Rows.Add(NewRow(1, Cell(heading.Id, null, "Description"), Cell(heading.Id, part, "Part")));

        var spec = NewSection(version, SpecName, SectionRole.Addable, 2, minimum: 0, maximum: null, initial: 0);
        spec.Rows.Add(NewRow(1, Cell(text.Id, null, null), Cell(number.Id, part, null)));

        db.TableTemplates.Add(template);
        await db.SaveChangesAsync(cancellationToken);
        return true;
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

    private static TemplateRow NewRow(int displayOrder, params TemplateCell[] cells)
    {
        var row = new TemplateRow { DisplayOrder = displayOrder };
        foreach (var cell in cells)
        {
            row.Cells.Add(cell);
        }

        return row;
    }

    private static TemplateCell Cell(int cellTypeId, TemplateColumnBlock? block, string? caption)
    {
        return new TemplateCell
        {
            CellTypeId = cellTypeId,
            TemplateColumnBlock = block,
            Column = 1,
            Caption = caption,
        };
    }
}
