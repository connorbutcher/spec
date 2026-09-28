using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

internal static class TableTemplateMappings
{
    public static TableTemplateSummaryDto ToSummaryDto(this TableTemplate template)
    {
        return new TableTemplateSummaryDto(
            template.Id,
            template.SheetTypeId,
            template.Name,
            template.DisplayOrder,
            template.Orientation);
    }

    /// <summary>
    /// Maps a template and its flat lists of sections and rows (with cells loaded) to the nested DTO.
    /// </summary>
    public static TableTemplateDto ToDto(
        this TableTemplate template,
        IReadOnlyList<TemplateSection> sections,
        IReadOnlyList<TemplateRow> rows)
    {
        var sectionsByParent = sections.ToLookup(section => section.ParentSectionId);
        var rowsBySection = rows.ToLookup(row => row.TemplateSectionId);

        return new TableTemplateDto(
            template.Id,
            template.SheetTypeId,
            template.Name,
            template.DisplayOrder,
            template.Orientation,
            MapSections(null, sectionsByParent, rowsBySection));
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
            .OrderBy(cell => cell.Column)
            .Select(cell => new TemplateCellDto(
                cell.Id,
                cell.CellTypeId,
                cell.Column,
                cell.RowSpan,
                cell.ColumnSpan,
                cell.Caption,
                cell.IsRequired))
            .ToList();

        return new TemplateRowDto(row.Id, row.DisplayOrder, cells);
    }
}
