using PUSpecSheet.Database;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// Lays out one version of a showcase template: its header, addable sections and sub-sections, column
/// blocks, rows and cells, each numbered in the order it is added. Cell types are named, not given by id.
/// </summary>
public sealed class TemplateLayout
{
    private readonly DbTableTemplateVersion version;
    private readonly ShowcaseCellTypeCatalog cellTypes;

    public TemplateLayout(DbTableTemplateVersion version, ShowcaseCellTypeCatalog cellTypes)
    {
        this.version = version;
        this.cellTypes = cellTypes;
    }

    public DbTableTemplateVersion Version => version;

    /// <summary>The table's one header section.</summary>
    public DbTemplateSection Header()
    {
        return Add(new DbTemplateSection { Name = "Header", Role = SectionRole.Header, MinInstances = 1, MaxInstances = 1, InitialInstances = 1 }, null);
    }

    /// <summary>An addable section at the top level, or inside <paramref name="parent"/>.</summary>
    public DbTemplateSection Section(string name, DbTemplateSection? parent, int minimum = 0, int? maximum = null, int initial = 0)
    {
        var section = new DbTemplateSection
        {
            Name = name,
            Role = SectionRole.Addable,
            MinInstances = minimum,
            MaxInstances = maximum,
            InitialInstances = Math.Max(minimum, initial),
        };
        return Add(section, parent);
    }

    /// <summary>A column block people add copies of across a horizontal table, to the right of the ones before it.</summary>
    public DbTemplateColumnBlock Block(string name, int minimum, int? maximum, int initial, int stickyColumns = 0)
    {
        var block = new DbTemplateColumnBlock
        {
            Name = name,
            DisplayOrder = version.ColumnBlocks.Count + 1,
            MinInstances = minimum,
            MaxInstances = maximum,
            InitialInstances = Math.Max(minimum, initial),
            StickyColumnCount = stickyColumns,
        };
        version.ColumnBlocks.Add(block);
        return block;
    }

    /// <summary>A row at the end of <paramref name="section"/>'s own rows.</summary>
    public DbTemplateRow Row(DbTemplateSection section, params DbTemplateCell[] cells)
    {
        if (!version.Sections.Contains(section))
        {
            throw new InvalidOperationException($"'{section.Name}' isn't a section of this template version.");
        }

        var row = new DbTemplateRow { DisplayOrder = section.Rows.Count + 1 };
        foreach (var cell in cells)
        {
            row.Cells.Add(cell);
        }

        section.Rows.Add(row);
        return row;
    }

    /// <summary>A cell of the cell type called <paramref name="cellType"/>, starting on <paramref name="column"/>.</summary>
    public DbTemplateCell Cell(
        string cellType,
        int column,
        string? caption = null,
        int columnSpan = 1,
        int rowSpan = 1,
        DbTemplateColumnBlock? block = null,
        CellStyle? style = null,
        CellConfiguration? configuration = null,
        string? lookupKey = null,
        bool required = false)
    {
        return new DbTemplateCell
        {
            CellTypeId = cellTypes[cellType],
            Column = column,
            ColumnSpan = columnSpan,
            RowSpan = rowSpan,
            Caption = caption,
            TemplateColumnBlock = block,
            StyleOverride = style,
            ConfigurationOverride = configuration,
            LookupKey = lookupKey,
            IsRequired = required,
        };
    }

    private DbTemplateSection Add(DbTemplateSection section, DbTemplateSection? parent)
    {
        section.DisplayOrder = parent is null
            ? version.Sections.Count(candidate => candidate.ParentSection is null) + 1
            : parent.ChildSections.Count + 1;
        version.Sections.Add(section);
        parent?.ChildSections.Add(section);
        if (parent is not null)
        {
            section.ParentSection = parent;
        }

        return section;
    }
}
