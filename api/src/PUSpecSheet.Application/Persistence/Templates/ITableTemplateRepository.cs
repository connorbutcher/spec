using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Persistence.Templates;

/// <summary>Table templates and their versions.</summary>
public interface ITableTemplateRepository
{
    /// <summary>A summary of every template, or of one sheet type's, with its latest version.</summary>
    Task<List<TableTemplateSummaryDto>> ListSummariesAsync(int? sheetTypeId, CancellationToken cancellationToken);

    /// <summary>The template, read only; null when there is no such template.</summary>
    Task<TableTemplate?> FindAsync(int id, CancellationToken cancellationToken);

    /// <summary>The template, tracked for editing; null when there is no such template.</summary>
    Task<TableTemplate?> FindForEditAsync(int id, CancellationToken cancellationToken);

    /// <summary>A sheet type's templates, tracked for editing.</summary>
    Task<List<TableTemplate>> ListForEditAsync(int sheetTypeId, CancellationToken cancellationToken);

    /// <summary>Whether another template of the sheet type than <paramref name="exceptId"/> already has the name.</summary>
    Task<bool> NameTakenAsync(int sheetTypeId, string name, int? exceptId, CancellationToken cancellationToken);

    /// <summary>The highest display order among a sheet type's templates, or null when it has none.</summary>
    Task<int?> GetLastDisplayOrderAsync(int sheetTypeId, CancellationToken cancellationToken);

    /// <summary>Whether any sheet table is built from any version of the template.</summary>
    Task<bool> IsUsedAsync(int templateId, CancellationToken cancellationToken);

    /// <summary>The template's versions, read only, oldest first.</summary>
    Task<List<TableTemplateVersion>> ListVersionsAsync(int templateId, CancellationToken cancellationToken);

    /// <summary>The template's latest version, tracked for editing.</summary>
    Task<TableTemplateVersion> GetLatestVersionForEditAsync(int templateId, CancellationToken cancellationToken);

    /// <summary>Which template the version belongs to and its number; null when there is no such version.</summary>
    Task<TemplateVersionKey?> FindVersionKeyAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>Whether the version can change; null when there is no such version.</summary>
    Task<TemplateVersionState?> FindVersionStateAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>The version's orientation. The version must exist.</summary>
    Task<TemplateOrientation> GetOrientationAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>Which of the versions a sheet table is built from.</summary>
    Task<List<int>> GetVersionIdsInUseAsync(IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken);

    void Add(TableTemplate tableTemplate);

    void Remove(TableTemplate tableTemplate);

    void AddVersion(TableTemplateVersion version);
}
