using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Published.Queries;
using PUSpecSheet.DataAccess.Published.Sheets;
using PUSpecSheet.DataAccess.Published.Versions;

namespace PUSpecSheet.DataAccess.Published;

public interface IPublishedSheetDataService
{
    Task<List<PublishedSheetReference>> ListReferencesAsync(
        string phaseCode,
        int sheetTypeId,
        IReadOnlyList<PublishedIdentifierFilter> filters,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid sheetPublicId, CancellationToken cancellationToken);

    Task<List<PublishedVersion>> ListVersionsAsync(Guid sheetPublicId, CancellationToken cancellationToken);

    Task<PublishedVersionStamp?> FindVersionAsync(Guid sheetPublicId, PublishedVersionPoint point, CancellationToken cancellationToken);

    IQueryable<PublishedVersionStamp> QueryVersions(Guid sheetPublicId, PublishedVersionPoint point);

    Task<List<PublishedVersionStamp>> ListVersionsUpToAsync(IReadOnlyCollection<int> sheetIds, DateTime moment, CancellationToken cancellationToken);
}
