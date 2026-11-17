using PUSpecSheet.Abstractions.Domain.SpecSheets;

namespace PUSpecSheet.Api.DAL.Caching;

internal interface ISpecSheetCache
{
    Task<SpecSheet?> GetAsync(int specSheetId, CancellationToken cancellationToken = default);

    Task SetAsync(SpecSheet specSheet, CancellationToken cancellationToken = default);

    Task RemoveAsync(int specSheetId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GetAllKeysAsync(CancellationToken cancellationToken = default);
}
