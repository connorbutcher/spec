using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Published.Versions;

namespace PUSpecSheet.DataAccess.Published.Rows;

public interface IPublishedRowsService
{
    Task<PublishedRows> ReadAsync(ResolvedSheetVersion version, PublishedRowsSelection selection, CancellationToken cancellationToken);

    Task<PublishedSheetRow> ReadRowAsync(ResolvedSheetVersion version, PublishedRowsSelection selection, CancellationToken cancellationToken);

    Task<ResolvedSheetVersion> ResolveAsync(Guid sheetPublicId, PublishedVersionPoint point, CancellationToken cancellationToken);
}
