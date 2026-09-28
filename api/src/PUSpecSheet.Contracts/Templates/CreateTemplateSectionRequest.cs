using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// Adds a section to the end of a template's top level, or of <see cref="ParentSectionId"/>'s children.
/// </summary>
public sealed record CreateTemplateSectionRequest(
    int TableTemplateId,
    int? ParentSectionId,
    [Required, MaxLength(100)] string Name);
