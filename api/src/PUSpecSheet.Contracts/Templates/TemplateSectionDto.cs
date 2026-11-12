using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// A section with its child sections nested and its own rows, which show before its child sections.
/// The instance counts say how many copies a sheet table starts with and may hold; a null maximum
/// means no limit. The header is always exactly one copy.
/// </summary>
public sealed record TemplateSectionDto
{
    public TemplateSectionDto(
        int id,
        int? parentSectionId,
        string name,
        int displayOrder,
        SectionRole role,
        int minInstances,
        int? maxInstances,
        int initialInstances,
        IReadOnlyList<TemplateSectionDto> sections,
        IReadOnlyList<TemplateRowDto> rows)
    {
        this.Id = id;
        this.ParentSectionId = parentSectionId;
        this.Name = name;
        this.DisplayOrder = displayOrder;
        this.Role = role;
        this.MinInstances = minInstances;
        this.MaxInstances = maxInstances;
        this.InitialInstances = initialInstances;
        this.Sections = sections;
        this.Rows = rows;
    }

    public int Id { get; init; }

    public int? ParentSectionId { get; init; }

    public string Name { get; init; }

    public int DisplayOrder { get; init; }

    public SectionRole Role { get; init; }

    public int MinInstances { get; init; }

    public int? MaxInstances { get; init; }

    public int InitialInstances { get; init; }

    public IReadOnlyList<TemplateSectionDto> Sections { get; init; }

    public IReadOnlyList<TemplateRowDto> Rows { get; init; }
}
