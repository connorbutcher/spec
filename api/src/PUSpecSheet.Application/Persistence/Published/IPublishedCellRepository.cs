using PUSpecSheet.Application.Published.Sheets;
using PUSpecSheet.Application.Published.Versions;

namespace PUSpecSheet.Application.Persistence.Published;

/// <summary>The cells of the rows that were on a sheet at a version, for the whole-sheet published API.</summary>
public interface IPublishedCellRepository
{
    /// <summary>The cells in scope (or the whole sheet for none), with each one's caption when asked.</summary>
    Task<IReadOnlyList<PublishedCellRecord>> ReadAsync(
        ResolvedSheetVersion version,
        PublishedSelectionScope? scope,
        bool includeCaptions,
        CancellationToken cancellationToken);
}
