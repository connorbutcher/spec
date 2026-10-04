using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Styles;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets.Demo;

/// <summary>
/// Adds the "Parts limits" sample template to the Parts sheet type: a horizontal table whose own column holds
/// the descriptions and whose "Part" column block, two columns wide, people add a copy of for each part. The
/// header has the part number across the top of each part, in blue, with Min and Max under it. Below it
/// people add rows of limits, each either a Min and Max for every part or a single value across both of a
/// part's columns. Does nothing if a template with that name already exists.
/// </summary>
public sealed class DemoPartsGridTemplateSeeder(PuSpecSheetDbContext db)
{
    public const string SheetTypeName = "Parts";
    public const string TemplateName = "Parts limits";
    public const string LimitsName = "Limits";
    public const string SingleValueName = "Single value";
    public const string PartBlockName = "Part";

    private static readonly CellStyle PartNumberStyle = new()
    {
        BackgroundColor = "#bfdbfe",
        TextColor = "#1e3a8a",
        Bold = true,
        Align = CellTextAlign.Center,
    };

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
            StickyColumnCount = 1,
            CreatedAtUtc = DateTime.UtcNow,
        };
        template.Versions.Add(version);

        var part = new TemplateColumnBlock
        {
            Name = PartBlockName,
            DisplayOrder = 1,
            MinInstances = 0,
            MaxInstances = null,
            InitialInstances = 4,
        };
        version.ColumnBlocks.Add(part);

        var header = NewSection(version, "Header", SectionRole.Header, 1, minimum: 1, maximum: 1, initial: 1);
        header.Rows.Add(NewRow(
            1,
            Cell(heading.Id, null, 1, "Description", rowSpan: 2),
            Cell(text.Id, part, 1, "Part number", columnSpan: 2, style: PartNumberStyle)));
        header.Rows.Add(NewRow(2, Cell(heading.Id, part, 1, "Min"), Cell(heading.Id, part, 2, "Max")));

        var limits = NewSection(version, LimitsName, SectionRole.Addable, 2, minimum: 0, maximum: null, initial: 0);
        limits.Rows.Add(NewRow(1, Cell(text.Id, null, 1, null), Cell(number.Id, part, 1, null), Cell(number.Id, part, 2, null)));

        var single = NewSection(version, SingleValueName, SectionRole.Addable, 3, minimum: 0, maximum: null, initial: 0);
        single.Rows.Add(NewRow(1, Cell(text.Id, null, 1, null), Cell(number.Id, part, 1, null, columnSpan: 2)));

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

    private static TemplateCell Cell(
        int cellTypeId,
        TemplateColumnBlock? block,
        int column,
        string? caption,
        int columnSpan = 1,
        int rowSpan = 1,
        CellStyle? style = null)
    {
        return new TemplateCell
        {
            CellTypeId = cellTypeId,
            TemplateColumnBlock = block,
            Column = column,
            ColumnSpan = columnSpan,
            RowSpan = rowSpan,
            Caption = caption,
            StyleOverride = style,
        };
    }
}
