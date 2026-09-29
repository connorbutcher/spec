using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// Adds a fixed section to the end of a template version's top level, or of
/// <see cref="ParentSectionId"/>'s children.
/// </summary>
public sealed record CreateTemplateSectionRequest(
    int TableTemplateVersionId,
    int? ParentSectionId,
    [Required, MaxLength(100)] string Name);
