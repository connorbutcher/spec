using System.Runtime.CompilerServices;
using PUSpecSheet.Abstractions.Domain.Sheets;

namespace PUSpecSheet.Api.DAL.Caching;

/// <summary>
/// A sheet cache that holds nothing, standing in for <see cref="SpecSheetCache"/> while Redis is disabled for local
/// running. Every read misses, so sheets are always read from the database.
/// </summary>
internal class NoOpSpecSheetCache : ISpecSheetCache
{
    public Task<Sheet?> GetAsync(int specSheetId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Sheet?>(null);
    }

    public Task SetAsync(Sheet sheet, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int specSheetId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<string> GetAllKeysAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield break;
    }
}
