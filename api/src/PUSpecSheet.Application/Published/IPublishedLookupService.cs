using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Finds published data by the value of a cell with a lookup key, such as a part number, for other
/// applications. Like the rest of the published API it is read-only and never returns drafts.
/// </summary>
public interface IPublishedLookupService
{
    /// <summary>
    /// Finds the cells that hold the value, and the sheet versions they are read at, in two small queries.
    /// Callers do this first so an unchanged answer can be recognised before any data is read.
    /// </summary>
    /// <param name="criteria">The key and value to find.</param>
    /// <param name="sheetPublicId">The one sheet to search, or null for every sheet.</param>
    /// <param name="point">The version of that sheet, or for every sheet the moment to read them at (now when not set).</param>
    /// <param name="cancellationToken">Stops the queries.</param>
    Task<PublishedLookupResolution> ResolveAsync(
        PublishedLookupCriteria criteria,
        Guid? sheetPublicId,
        PublishedVersionPoint point,
        CancellationToken cancellationToken);

    /// <summary>Reads what belongs to each cell that was found.</summary>
    Task<PublishedLookupDto> ReadAsync(PublishedLookupResolution resolution, CancellationToken cancellationToken);
}
