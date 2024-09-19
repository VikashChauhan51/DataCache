using DataCache.Abstraction;
using DataCache.Configurations;

namespace DataCache.Cache;


/// <summary>
/// A cache implementation that supports asynchronous operations and eviction strategies.
/// </summary>
public class Cache<TKey, TValue> : ICacheAsync<TKey, TValue> where TKey : notnull, IEquatable<TKey>
{

    protected readonly CacheOptions _cacheOptions;

    private readonly IDataProviderAsync<TKey, TValue> _provider;

    public Cache(IDataProviderAsync<TKey, TValue> provider, CacheOptions cacheOptions)
    {
        _provider = provider;
        _cacheOptions = cacheOptions;
    }

    /// <inheritdoc />
    public async Task<TValue> GetAsync(TKey key)
    {
        return await _provider.GetAsync(key);
    }

    /// <inheritdoc />
    public async Task<TValue> GetOrSetAsync(TKey key, Func<Task<TValue>> valueFactory, TimeSpan? ttl)
    {
        var item = await _provider.GetAsync(key);
        if (item == null)
        {
            item = await valueFactory();

            await SetAsync(key, item, ttl);
        }
        return item;
    }

    /// <inheritdoc />
    public async Task SetAsync(TKey key, TValue value, TimeSpan? ttl)
    {
        await _provider.AddAsync(key, value, ttl.HasValue ? ttl : _cacheOptions.DefaultTTL);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(TKey key)
    {
        await _provider.RemoveAsync(key);
    }
}
