using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets.Demo;

/// <summary>
/// Adds the "Limits table" sample template to the Specification sheet type: a header of column names, an
/// addable group (one row with a single cell across all five columns) and, inside a group, two kinds of
/// addable sub-section: one with a description and four limits, one with a description and a single value
/// across four columns. Does nothing if a template with that name already exists.
/// </summary>
public sealed class DemoLimitsTemplateSeeder(PuSpecSheetDbContext db)
{
    public const string SheetTypeName = "Specification";
    public const string TemplateName = "Limits table";
    public const string GroupName = "Group";
    public const string LimitsName = "Limits";
    public const string SingleValueName = "Single value";

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
            Orientation = TemplateOrientation.Vertical,
            CreatedAtUtc = DateTime.UtcNow,
        };
        template.Versions.Add(version);

        var header = NewSection(version, null, "Header", SectionRole.Header, 1, minimum: 1, maximum: 1, initial: 1);
        header.Rows.Add(NewRow(1,
            Cell(heading.Id, 1, 1, "Description"),
            Cell(heading.Id, 2, 1, "Lower"),
            Cell(heading.Id, 3, 1, "Min"),
            Cell(heading.Id, 4, 1, "Max"),
            Cell(heading.Id, 5, 1, "Upper")));

        var group = NewSection(version, null, GroupName, SectionRole.Addable, 2, minimum: 0, maximum: null, initial: 0);
        group.Rows.Add(NewRow(1, Cell(text.Id, 1, 5, "Group")));

        var limits = NewSection(version, group, LimitsName, SectionRole.Addable, 1, minimum: 0, maximum: null, initial: 0);
        limits.Rows.Add(NewRow(1,
            Cell(text.Id, 1, 1, null),
            Cell(number.Id, 2, 1, null),
            Cell(number.Id, 3, 1, null),
            Cell(number.Id, 4, 1, null),
            Cell(number.Id, 5, 1, null)));

        var single = NewSection(version, group, SingleValueName, SectionRole.Addable, 2, minimum: 0, maximum: null, initial: 0);
        single.Rows.Add(NewRow(1,
            Cell(text.Id, 1, 1, null),
            Cell(text.Id, 2, 4, null)));

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
        TemplateSection? parent,
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
        parent?.ChildSections.Add(section);
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

    private static TemplateCell Cell(int cellTypeId, int column, int columnSpan, string? caption)
    {
        return new TemplateCell
        {
            CellTypeId = cellTypeId,
            Column = column,
            ColumnSpan = columnSpan,
            Caption = caption,
        };
    }
}
