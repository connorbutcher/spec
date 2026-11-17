using Microsoft.EntityFrameworkCore;
using PUSpecSheet.DataAccess.Published.Sheets;
using PUSpecSheet.DataAccess.Published.Versions;

namespace PUSpecSheet.DataAccess.Published;

public interface IPublishedCellDataService
{
    Task<IReadOnlyList<PublishedCellRecord>> ReadAsync(
        ResolvedSheetVersion version,
        PublishedSelectionScope? scope,
        bool includeCaptions,
        CancellationToken cancellationToken);

    IQueryable<PublishedCellRecord>? Query(ResolvedSheetVersion version, PublishedSelectionScope? scope, bool includeCaptions);
}
