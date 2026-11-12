using PUSpecSheet.Application.Published;

namespace PUSpecSheet.Application.Persistence.Published;

/// <summary>The values of lookup and row cells, read only from the value table of each cell's kind.</summary>
public interface IPublishedKindValueRepository
{
    /// <summary>
    /// The values of the cells, by cell id. With <paramref name="wholeSheet"/> the row revisions are found
    /// by the sheet in the database rather than sent as a list.
    /// </summary>
    Task<Dictionary<int, object>> ReadAsync(
        IReadOnlyList<PublishedLookupCellRecord> cells,
        ResolvedSheetVersion? wholeSheet,
        CancellationToken cancellationToken);
}
