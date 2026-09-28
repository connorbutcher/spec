namespace PUSpecSheet.Contracts.Templates;

/// <summary>A section with its child sections nested. Only a section without children has rows.</summary>
public sealed record TemplateSectionDto(
    int Id,
    int? ParentSectionId,
    string Name,
    int DisplayOrder,
    IReadOnlyList<TemplateSectionDto> Sections,
    IReadOnlyList<TemplateRowDto> Rows);
