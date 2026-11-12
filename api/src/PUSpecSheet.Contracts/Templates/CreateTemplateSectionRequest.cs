using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// Adds a fixed section to the end of a template version's top level, or of
/// <see cref="ParentSectionId"/>'s children.
/// </summary>
public sealed record CreateTemplateSectionRequest
{
    public CreateTemplateSectionRequest(int tableTemplateVersionId, int? parentSectionId, [Required, MaxLength(100)] string name)
    {
        this.TableTemplateVersionId = tableTemplateVersionId;
        this.ParentSectionId = parentSectionId;
        this.Name = name;
    }

    public int TableTemplateVersionId { get; init; }

    public int? ParentSectionId { get; init; }

    public string Name { get; init; }
}
