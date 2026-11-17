using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Templates;

/// <summary>
/// Adds a fixed section to the end of a template version's top level, or of
/// <see cref="ParentSectionId"/>'s children.
/// </summary>
public sealed record CreateTemplateSectionRequest
{
    public required int TableTemplateVersionId { get; init; }

    public int? ParentSectionId { get; init; }

    [Required, MaxLength(100)]
    public required string Name { get; init; }
}
