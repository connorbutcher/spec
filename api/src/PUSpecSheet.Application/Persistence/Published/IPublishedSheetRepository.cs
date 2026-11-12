using PUSpecSheet.Application.Published.Versions;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Persistence.Published;

/// <summary>Sheets and their versions as other applications find them: by phase code and public identifier.</summary>
public interface IPublishedSheetRepository
{
    /// <summary>The phase's sheet of the sheet type and its latest version; null when it has none.</summary>
    Task<PublishedSheetReferenceDto?> FindReferenceAsync(string phaseCode, int sheetTypeId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid sheetPublicId, CancellationToken cancellationToken);

    /// <summary>The sheet's versions, oldest first.</summary>
    Task<List<PublishedVersionDto>> ListVersionsAsync(Guid sheetPublicId, CancellationToken cancellationToken);

    /// <summary>
    /// The version a point names: the version with its number, else the last one published at or before its
    /// moment, else the latest; null when there is none.
    /// </summary>
    Task<PublishedVersionStamp?> FindVersionAsync(Guid sheetPublicId, PublishedVersionPoint point, CancellationToken cancellationToken);

    /// <summary>Every version of the sheets published at or before the moment.</summary>
    Task<List<PublishedVersionStamp>> ListVersionsUpToAsync(IReadOnlyCollection<int> sheetIds, DateTime moment, CancellationToken cancellationToken);
}
