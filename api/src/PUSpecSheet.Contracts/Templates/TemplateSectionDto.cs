using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// A section with its child sections nested. Only a section without children has rows. The instance
/// counts say how many copies a sheet table starts with and may hold; a null maximum means no limit.
/// </summary>
public sealed record TemplateSectionDto(
    int Id,
    int? ParentSectionId,
    string Name,
    int DisplayOrder,
    SectionRole Role,
    int MinInstances,
    int? MaxInstances,
    int InitialInstances,
    IReadOnlyList<TemplateSectionDto> Sections,
    IReadOnlyList<TemplateRowDto> Rows);
