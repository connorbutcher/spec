using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.Abstractions.CellTypes.Styles;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Seeding.ReferenceData;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Seeding.Templates;

public sealed class TemplateLayout
{
    private readonly DbTableTemplateVersion _version;
    private readonly ShowcaseCellTypeCatalog _cellTypes;

    public TemplateLayout(DbTableTemplateVersion version, ShowcaseCellTypeCatalog cellTypes)
    {
        _version = version;
        _cellTypes = cellTypes;
    }

    public DbTableTemplateVersion Version => _version;

    public DbTemplateSection Header()
    {
        return Add(new DbTemplateSection { Name = "Header", Role = SectionRole.Header, MinInstances = 1, MaxInstances = 1, InitialInstances = 1 }, null);
    }

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

    public DbTemplateColumnBlock Block(string name, int minimum, int? maximum, int initial, int stickyColumns = 0)
    {
        var block = new DbTemplateColumnBlock
        {
            Name = name,
            DisplayOrder = _version.ColumnBlocks.Count + 1,
            MinInstances = minimum,
            MaxInstances = maximum,
            InitialInstances = Math.Max(minimum, initial),
            StickyColumnCount = stickyColumns,
        };
        _version.ColumnBlocks.Add(block);
        return block;
    }

    public DbTemplateRow Row(DbTemplateSection section, params DbTemplateCell[] cells)
    {
        if (!_version.Sections.Contains(section))
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
            CellTypeId = _cellTypes[cellType],
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
            ? _version.Sections.Count(candidate => candidate.ParentSection is null) + 1
            : parent.ChildSections.Count + 1;
        _version.Sections.Add(section);
        parent?.ChildSections.Add(section);
        if (parent is not null)
        {
            section.ParentSection = parent;
        }

        return section;
    }
}
