namespace PUSpecSheet.Abstractions.Templates;

/// <summary>
/// A section with its child sections nested and its own rows, which show before its child sections.
/// The instance counts say how many copies a sheet table starts with and may hold; a null maximum
/// means no limit. The header is always exactly one copy.
/// </summary>
public sealed record TemplateSection
{
    public required int Id { get; init; }

    public int? ParentSectionId { get; init; }

    public required string Name { get; init; }

    public required int DisplayOrder { get; init; }

    public required SectionRole Role { get; init; }

    public required int MinInstances { get; init; }

    public int? MaxInstances { get; init; }

    public required int InitialInstances { get; init; }

    public required IReadOnlyList<TemplateSection> Sections { get; init; }

    public required IReadOnlyList<TemplateRow> Rows { get; init; }
}
