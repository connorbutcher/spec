namespace PUSpecSheet.Api.Common.Redis;

public interface ICacheProvider<TValue>
{
    IAsyncEnumerable<string> GetAllKeysAsync(string? searchPrefix, CancellationToken cancellationToken = default);

    Task<long> GetKeyCountAsync(string? searchPrefix, CancellationToken cancellationToken = default);

    IAsyncEnumerable<TValue> GetAllValuesAsync(string? searchPrefix, CancellationToken cancellationToken = default);

    Task<TValue?> GetAsync(string key, CancellationToken token = default);

    Task<bool> TrySetAsync(string key, TValue value, CancellationToken token = default);

    Task<bool> TryUpdateAsync(string key, TValue value, CancellationToken token = default);

    Task RemoveAsync(string key, CancellationToken token = default);
}
