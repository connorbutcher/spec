using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Published;

/// <summary>Reads rows of a published sheet by row identifier, for other applications.</summary>
public interface IPublishedRowsService
{
    /// <summary>The selected rows of the sheet at a resolved version, by row identifier.</summary>
    Task<PublishedRowsDto> ReadAsync(ResolvedSheetVersion version, PublishedRowsSelection selection, CancellationToken cancellationToken);
}
