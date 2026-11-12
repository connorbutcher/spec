using PUSpecSheet.Application.Published.Lookups;
using PUSpecSheet.Application.Published.Versions;

namespace PUSpecSheet.Application.Persistence.Published;

/// <summary>Cells of a published sheet with their kind, for lookups and the rows API.</summary>
public interface IPublishedLookupCellRepository
{
    /// <summary>The cells around a lookup hit: its row, or the sections given, in its table.</summary>
    Task<List<PublishedLookupCellRecord>> ListAroundHitAsync(
        ResolvedSheetVersion version,
        PublishedLookupHit hit,
        IReadOnlyList<int>? sectionIds,
        CancellationToken cancellationToken);

    /// <summary>Every cell on the sheet at the version.</summary>
    Task<List<PublishedLookupCellRecord>> ListOnSheetAsync(ResolvedSheetVersion version, CancellationToken cancellationToken);

    /// <summary>The cells of the rows with the public identifiers.</summary>
    Task<List<PublishedLookupCellRecord>> ListInRowsAsync(ResolvedSheetVersion version, IReadOnlyList<Guid> rowPublicIds, CancellationToken cancellationToken);

    /// <summary>The header cells of the tables.</summary>
    Task<List<PublishedLookupCellRecord>> ListHeadersAsync(ResolvedSheetVersion version, IReadOnlyList<int> tableIds, CancellationToken cancellationToken);
}
