using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Published.Versions;

namespace PUSpecSheet.DataAccess.Published.Sheets;

// Never returns drafts, locks or anything that depends on who is asking: other applications rely on it.
public interface IPublishedSheetsService
{
    Task<PublishedSheetReference> FindAsync(string phaseCode, int sheetTypeId, IReadOnlyDictionary<string, string> identifiers, CancellationToken cancellationToken);

    Task<IReadOnlyList<PublishedSheetReference>> ListAsync(string phaseCode, int sheetTypeId, IReadOnlyDictionary<string, string> identifiers, CancellationToken cancellationToken);

    Task<IReadOnlyList<PublishedVersion>> VersionsAsync(Guid sheetPublicId, CancellationToken cancellationToken);

    Task<ResolvedSheetVersion> ResolveAsync(Guid sheetPublicId, PublishedVersionPoint point, CancellationToken cancellationToken);

    Task<PublishedSheet> ReadAsync(ResolvedSheetVersion version, PublishedSheetSelection selection, CancellationToken cancellationToken);
}
