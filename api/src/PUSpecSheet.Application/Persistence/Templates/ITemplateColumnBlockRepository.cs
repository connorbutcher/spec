using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Persistence.Templates;

/// <summary>The column blocks of horizontal template versions.</summary>
public interface ITemplateColumnBlockRepository
{
    /// <summary>A version's column blocks, read only.</summary>
    Task<List<TemplateColumnBlock>> ListAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>A version's column blocks, tracked for editing.</summary>
    Task<List<TemplateColumnBlock>> ListForEditAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>The ids of a version's column blocks, in display order.</summary>
    Task<List<int>> ListIdsAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>The column block, tracked for editing; null when there is no such block.</summary>
    Task<TemplateColumnBlock?> FindForEditAsync(int id, CancellationToken cancellationToken);

    Task<bool> AnyAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>Whether the column block is part of the version.</summary>
    Task<bool> IsInVersionAsync(int id, int versionId, CancellationToken cancellationToken);

    /// <summary>The highest display order among a version's column blocks, or null when it has none.</summary>
    Task<int?> GetLastDisplayOrderAsync(int versionId, CancellationToken cancellationToken);

    void Add(TemplateColumnBlock block);

    void Remove(TemplateColumnBlock block);
}
