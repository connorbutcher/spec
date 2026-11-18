using PUSpecSheet.Abstractions.Domain.Sheets;

namespace PUSpecSheet.Api.DAL.Caching;

/// <summary>
/// Published sheets as they stood at a version, by the id of the SpecSheets row the version came from.
/// </summary>
internal interface ISpecSheetCache
{
    Task<Sheet?> GetAsync(int specSheetId, CancellationToken cancellationToken = default);

    Task SetAsync(Sheet sheet, CancellationToken cancellationToken = default);

    Task RemoveAsync(int specSheetId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GetAllKeysAsync(CancellationToken cancellationToken = default);
}
