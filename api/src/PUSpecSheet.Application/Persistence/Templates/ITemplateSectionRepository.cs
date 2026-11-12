using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Persistence.Templates;

/// <summary>The sections of template versions.</summary>
public interface ITemplateSectionRepository
{
    /// <summary>A version's sections, read only.</summary>
    Task<List<TemplateSection>> ListAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>A version's sections with their rows and cells, read only.</summary>
    Task<List<TemplateSection>> ListWithRowsAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>A version's sections, tracked for editing.</summary>
    Task<List<TemplateSection>> ListForEditAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>The sections under one parent (or at the top level) of a version, tracked for editing.</summary>
    Task<List<TemplateSection>> ListSiblingsForEditAsync(int versionId, int? parentSectionId, CancellationToken cancellationToken);

    /// <summary>The section, tracked for editing; null when there is no such section.</summary>
    Task<TemplateSection?> FindForEditAsync(int id, CancellationToken cancellationToken);

    /// <summary>The version the section is in; null when there is no such section.</summary>
    Task<int?> FindVersionIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>The role of a section of the version; null when the version has no such section.</summary>
    Task<SectionRole?> FindRoleAsync(int versionId, int id, CancellationToken cancellationToken);

    /// <summary>The highest display order under one parent (or at the top level) of a version, or null when there are none.</summary>
    Task<int?> GetLastDisplayOrderAsync(int versionId, int? parentSectionId, CancellationToken cancellationToken);

    void Add(TemplateSection section);

    void RemoveRange(IEnumerable<TemplateSection> sections);
}
