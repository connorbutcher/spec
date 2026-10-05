using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

/// <summary>Deep-copies a template version (column blocks, sections, rows and cells) into the next version number.</summary>
internal static class TemplateVersionCopier
{
    /// <summary>Adds the copy to the context; the caller saves it.</summary>
    public static async Task<TableTemplateVersion> CopyAsync(
        PuSpecSheetDbContext db,
        TableTemplateVersion source,
        int newVersionNumber,
        CancellationToken cancellationToken)
    {
        var sections = await db.TemplateSections
            .AsNoTracking()
            .Where(section => section.TableTemplateVersionId == source.Id)
            .Include(section => section.Rows)
            .ThenInclude(row => row.Cells)
            .ToListAsync(cancellationToken);

        var columnBlocks = await db.TemplateColumnBlocks
            .AsNoTracking()
            .Where(block => block.TableTemplateVersionId == source.Id)
            .ToListAsync(cancellationToken);

        var copy = new TableTemplateVersion
        {
            TableTemplateId = source.TableTemplateId,
            VersionNumber = newVersionNumber,
            Orientation = source.Orientation,
            StickyColumnCount = source.StickyColumnCount,
        };

        var blockCopies = columnBlocks.ToDictionary(block => block.Id, block => CopyColumnBlock(block, copy));

        var copies = sections.ToDictionary(section => section.Id, section => CopySection(section, copy, blockCopies));
        foreach (var section in sections.Where(section => section.ParentSectionId is not null))
        {
            copies[section.Id].ParentSection = copies[section.ParentSectionId!.Value];
        }

        db.TableTemplateVersions.Add(copy);
        return copy;
    }

    private static TemplateColumnBlock CopyColumnBlock(TemplateColumnBlock block, TableTemplateVersion version)
    {
        var copy = new TemplateColumnBlock
        {
            TableTemplateVersion = version,
            Name = block.Name,
            DisplayOrder = block.DisplayOrder,
            MinInstances = block.MinInstances,
            MaxInstances = block.MaxInstances,
            InitialInstances = block.InitialInstances,
            StickyColumnCount = block.StickyColumnCount,
        };

        version.ColumnBlocks.Add(copy);
        return copy;
    }

    private static TemplateSection CopySection(
        TemplateSection section,
        TableTemplateVersion version,
        Dictionary<int, TemplateColumnBlock> blockCopies)
    {
        var copy = new TemplateSection
        {
            TableTemplateVersion = version,
            Name = section.Name,
            DisplayOrder = section.DisplayOrder,
            Role = section.Role,
            MinInstances = section.MinInstances,
            MaxInstances = section.MaxInstances,
            InitialInstances = section.InitialInstances,
            Rows = section.Rows
                .Select(row => new TemplateRow
                {
                    DisplayOrder = row.DisplayOrder,
                    Cells = row.Cells
                        .Select(cell => new TemplateCell
                        {
                            TemplateColumnBlock = cell.TemplateColumnBlockId is int blockId ? blockCopies[blockId] : null,
                            Column = cell.Column,
                            RowSpan = cell.RowSpan,
                            ColumnSpan = cell.ColumnSpan,
                            CellTypeId = cell.CellTypeId,
                            Caption = cell.Caption,
                            IsRequired = cell.IsRequired,
                            ConfigurationOverride = cell.ConfigurationOverride,
                            StyleOverride = cell.StyleOverride,
                            LookupKey = cell.LookupKey,
                        })
                        .ToList(),
                })
                .ToList(),
        };

        version.Sections.Add(copy);
        return copy;
    }
}
