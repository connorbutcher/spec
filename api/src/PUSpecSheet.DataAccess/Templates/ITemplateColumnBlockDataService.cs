using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates;

public interface ITemplateColumnBlockDataService
{
    Task<List<DbTemplateColumnBlock>> ListAsync(int versionId, CancellationToken cancellationToken);

    Task<List<DbTemplateColumnBlock>> ListForEditAsync(int versionId, CancellationToken cancellationToken);

    Task<List<int>> ListIdsAsync(int versionId, CancellationToken cancellationToken);

    Task<DbTemplateColumnBlock?> FindForEditAsync(int id, CancellationToken cancellationToken);

    Task<bool> AnyAsync(int versionId, CancellationToken cancellationToken);

    Task<bool> IsInVersionAsync(int id, int versionId, CancellationToken cancellationToken);

    Task<int?> GetLastDisplayOrderAsync(int versionId, CancellationToken cancellationToken);

    Task<DbTemplateColumnBlock?> FindInVersionAsync(int id, int versionId, CancellationToken cancellationToken);

    Task<List<DbTemplateColumnBlock>> ListInVersionsAsync(IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken);
}
