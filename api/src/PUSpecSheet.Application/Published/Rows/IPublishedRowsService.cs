using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.Application.Published.Versions;

namespace PUSpecSheet.Application.Published.Rows;

/// <summary>Reads rows of a published sheet by row identifier, for other applications.</summary>
public interface IPublishedRowsService
{
    /// <summary>The selected rows of the sheet at a resolved version, by row identifier.</summary>
    Task<PublishedRowsDto> ReadAsync(ResolvedSheetVersion version, PublishedRowsSelection selection, CancellationToken cancellationToken);

    /// <summary>
    /// The one row a selection of one row names, at a resolved version. Fails when the version doesn't hold it.
    /// </summary>
    Task<PublishedSheetRowDto> ReadRowAsync(ResolvedSheetVersion version, PublishedRowsSelection selection, CancellationToken cancellationToken);

    /// <summary>
    /// Finds the version a caller means, in one small query. Callers do this first so an unchanged answer
    /// can be recognised before any data is read.
    /// </summary>
    Task<ResolvedSheetVersion> ResolveAsync(Guid sheetPublicId, PublishedVersionPoint point, CancellationToken cancellationToken);
}
