using DataCache.Abstraction;
using DataCache.Configurations;
using DataCache.DataProviders;

namespace DataCache.Cache;

public class HybridCache<TKey, TValue> : ICacheAsync<TKey, TValue> where TKey : notnull, IEquatable<TKey>
{
    protected readonly CacheOptions _cacheOptions;

    private readonly IDataProviderAsync<TKey, TValue> _provider;
    private readonly InMemoryDataProvider<TKey, TValue> _inMemoryDataProvider;
    public HybridCache(IDataProviderAsync<TKey, TValue> provider, CacheOptions cacheOptions, IEvictionStrategy<TKey> evictionStrategy, Func<TValue, long> sizeCalculator)
    {
        _provider = provider;
        _cacheOptions = cacheOptions;
        _inMemoryDataProvider = new InMemoryDataProvider<TKey, TValue>(evictionStrategy, sizeCalculator);
    }

    /// <inheritdoc />
    public async Task<TValue> GetAsync(TKey key)
    {
        var cacheItem = await _inMemoryDataProvider.GetAsync(key);
        if (cacheItem != null)
        {
            return cacheItem;
        }

        return await _provider.GetAsync(key);
    }

    /// <inheritdoc />
    public async Task<TValue> GetOrSetAsync(TKey key, Func<Task<TValue>> valueFactory, TimeSpan? ttl)
    {
        var item = await GetAsync(key);
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
        await _inMemoryDataProvider.AddAsync(key, value, ttl.HasValue ? ttl : _cacheOptions.DefaultTTL);
        await _provider.AddAsync(key, value, ttl.HasValue ? ttl : _cacheOptions.DefaultTTL);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(TKey key)
    {
        await _inMemoryDataProvider.RemoveAsync(key);
        await _provider.RemoveAsync(key);
    }

}
