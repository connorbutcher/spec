using PUSpecSheet.Application.Published.Sheets;
using PUSpecSheet.Application.Published.Versions;

namespace PUSpecSheet.Application.Persistence.Published;

/// <summary>The values the cells of a published sheet held, as the published API writes them.</summary>
public interface IPublishedValueRepository
{
    /// <summary>
    /// The values of the cells, by cell id. With <paramref name="wholeSheet"/> the row revisions are found
    /// by the sheet in the database rather than sent as a list.
    /// </summary>
    Task<Dictionary<int, object>> ReadAsync(
        IReadOnlyList<PublishedCellRecord> cells,
        ResolvedSheetVersion? wholeSheet,
        CancellationToken cancellationToken);
}
