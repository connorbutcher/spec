using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.Database.Mappers.Templates;

/// <summary>Table templates, their versions and what is in them as DTOs.</summary>
public static class TableTemplateMapper
{
    /// <summary>
    /// A template at one version, with that version's flat lists of sections, rows (cells loaded) and column
    /// blocks, as the nested DTO.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="version">The version shown.</param>
    /// <param name="isEditable">Whether the version can still be changed: the latest, and no sheet uses it.</param>
    /// <param name="versions">Every version of the template, oldest first.</param>
    /// <param name="sections">The version's sections.</param>
    /// <param name="rows">The version's rows, with their cells.</param>
    /// <param name="columnBlocks">The version's column blocks.</param>
    public static TableTemplateDto ToDto(
        this DbTableTemplate template,
        DbTableTemplateVersion version,
        bool isEditable,
        IReadOnlyList<TableTemplateVersionSummaryDto> versions,
        IReadOnlyList<DbTemplateSection> sections,
        IReadOnlyList<DbTemplateRow> rows,
        IReadOnlyList<DbTemplateColumnBlock> columnBlocks)
    {
        var sectionsByParent = sections.ToLookup(section => section.ParentSectionId);
        var rowsBySection = rows.ToLookup(row => row.TemplateSectionId);

        return new TableTemplateDto(
            template.Id,
            template.SheetTypeId,
            template.Name,
            template.DisplayOrder,
            version.Id,
            version.VersionNumber,
            version.Orientation,
            version.StickyColumnCount,
            isEditable,
            versions,
            SectionsUnder(null, sectionsByParent, rowsBySection),
            columnBlocks
                .OrderBy(block => block.DisplayOrder)
                .Select(block => block.ToDto())
                .ToList());
    }

    /// <param name="version">The version.</param>
    /// <param name="isInUse">Whether a sheet table is built from it, which freezes it.</param>
    public static TableTemplateVersionSummaryDto ToSummaryDto(this DbTableTemplateVersion version, bool isInUse)
    {
        return new TableTemplateVersionSummaryDto(version.Id, version.VersionNumber, version.CreatedAtUtc, isInUse);
    }

    public static TemplateColumnBlockDto ToDto(this DbTemplateColumnBlock block)
    {
        return new TemplateColumnBlockDto(
            block.Id,
            block.Name,
            block.DisplayOrder,
            block.MinInstances,
            block.MaxInstances,
            block.InitialInstances,
            block.StickyColumnCount);
    }

    /// <summary>Expects <see cref="DbTemplateRow.Cells"/> to be loaded.</summary>
    public static TemplateRowDto ToDto(this DbTemplateRow row)
    {
        var cells = row.Cells
            .OrderBy(cell => cell.TemplateColumnBlockId ?? 0)
            .ThenBy(cell => cell.Column)
            .Select(cell => cell.ToDto())
            .ToList();

        return new TemplateRowDto(row.Id, row.DisplayOrder, cells);
    }

    public static TemplateCellDto ToDto(this DbTemplateCell cell)
    {
        return new TemplateCellDto(
            cell.Id,
            cell.CellTypeId,
            cell.Column,
            cell.RowSpan,
            cell.ColumnSpan,
            cell.Caption,
            cell.IsRequired,
            cell.ConfigurationOverride,
            cell.StyleOverride,
            cell.TemplateColumnBlockId,
            cell.LookupKey);
    }

    private static List<TemplateSectionDto> SectionsUnder(
        int? parentId,
        ILookup<int?, DbTemplateSection> sectionsByParent,
        ILookup<int, DbTemplateRow> rowsBySection)
    {
        return sectionsByParent[parentId]
            .OrderBy(section => section.DisplayOrder)
            .Select(section => new TemplateSectionDto(
                section.Id,
                section.ParentSectionId,
                section.Name,
                section.DisplayOrder,
                section.Role,
                section.MinInstances,
                section.MaxInstances,
                section.InitialInstances,
                SectionsUnder(section.Id, sectionsByParent, rowsBySection),
                rowsBySection[section.Id]
                    .OrderBy(row => row.DisplayOrder)
                    .Select(row => row.ToDto())
                    .ToList()))
            .ToList();
    }
}
