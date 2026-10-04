using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

internal static class TableTemplateMappings
{
    /// <summary>
    /// Maps a template at one version, with that version's flat lists of sections and rows (cells
    /// loaded) and its column blocks, to the nested DTO.
    /// </summary>
    public static TableTemplateDto ToDto(
        this TableTemplate template,
        TableTemplateVersion version,
        bool isEditable,
        IReadOnlyList<TableTemplateVersionSummaryDto> versions,
        IReadOnlyList<TemplateSection> sections,
        IReadOnlyList<TemplateRow> rows,
        IReadOnlyList<TemplateColumnBlock> columnBlocks)
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
            MapSections(null, sectionsByParent, rowsBySection),
            columnBlocks
                .OrderBy(block => block.DisplayOrder)
                .Select(block => new TemplateColumnBlockDto(
                    block.Id,
                    block.Name,
                    block.DisplayOrder,
                    block.MinInstances,
                    block.MaxInstances,
                    block.InitialInstances,
                    block.StickyColumnCount))
                .ToList());
    }

    private static List<TemplateSectionDto> MapSections(
        int? parentId,
        ILookup<int?, TemplateSection> sectionsByParent,
        ILookup<int, TemplateRow> rowsBySection)
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
                MapSections(section.Id, sectionsByParent, rowsBySection),
                rowsBySection[section.Id]
                    .OrderBy(row => row.DisplayOrder)
                    .Select(row => row.ToDto())
                    .ToList()))
            .ToList();
    }

    private static TemplateRowDto ToDto(this TemplateRow row)
    {
        var cells = row.Cells
            .OrderBy(cell => cell.TemplateColumnBlockId ?? 0)
            .ThenBy(cell => cell.Column)
            .Select(cell => new TemplateCellDto(
                cell.Id,
                cell.CellTypeId,
                cell.Column,
                cell.RowSpan,
                cell.ColumnSpan,
                cell.Caption,
                cell.IsRequired,
                cell.ConfigurationOverride,
                cell.StyleOverride,
                cell.TemplateColumnBlockId))
            .ToList();

        return new TemplateRowDto(row.Id, row.DisplayOrder, cells);
    }
}
