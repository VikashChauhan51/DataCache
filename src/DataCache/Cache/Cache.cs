using DataCache.Abstraction;
using DataCache.Configurations;

namespace DataCache.Cache;

/// <summary>
/// A cache implementation that supports asynchronous operations and eviction strategies.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify cache items. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
/// <typeparam name="TValue">The type of the value to be stored in the cache.</typeparam>
public class Cache<TKey, TValue> : ICacheAsync<TKey, TValue>
    where TKey : notnull, IEquatable<TKey>
{
    private readonly CacheOptions cacheOptions;

    private readonly IDataProviderAsync<TKey, TValue> provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cache{TKey, TValue}"/> class, which provides a caching mechanism
    /// with customizable options and a data provider for managing cache entries.
    /// </summary>
    /// <param name="provider">The data provider responsible for interacting with the underlying data source for cache operations.
    /// This provider defines how cache items are stored and retrieved.</param>
    /// <param name="cacheOptions">The configuration options for the cache, including settings such as the maximum memory size and default TTL (Time-To-Live) for cache entries.</param>
    public Cache(IDataProviderAsync<TKey, TValue> provider, CacheOptions cacheOptions)
    {
        this.provider = provider;
        this.cacheOptions = cacheOptions;
    }

    /// <inheritdoc />
    public async Task<TValue> GetAsync(TKey key)
    {
        return await this.provider.GetAsync(key);
    }

    /// <inheritdoc />
    public async Task<TValue> GetOrSetAsync(TKey key, Func<Task<TValue>> valueFactory, TimeSpan? ttl)
    {
        var item = await this.provider.GetAsync(key);
        if (item == null)
        {
            item = await valueFactory();

            await this.SetAsync(key, item, ttl);
        }

        return item;
    }

    /// <inheritdoc />
    public async Task SetAsync(TKey key, TValue value, TimeSpan? ttl)
    {
        await this.provider.AddAsync(key, value, ttl.HasValue ? ttl : this.cacheOptions.DefaultTTL);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(TKey key)
    {
        await this.provider.RemoveAsync(key);
    }
}
