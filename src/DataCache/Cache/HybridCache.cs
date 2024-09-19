using DataCache.Abstraction;
using DataCache.Configurations;
using DataCache.DataProviders;

namespace DataCache.Cache;

/// <summary>
/// Represents a hybrid cache that combines in-memory caching with a persistent data provider for efficient cache management.
/// The hybrid cache leverages both fast in-memory storage and a persistent data provider to reduce frequent data access costs.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify cache items. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
/// <typeparam name="TValue">The type of the value to be stored in the cache.</typeparam>
public class HybridCache<TKey, TValue> : ICacheAsync<TKey, TValue>
    where TKey : notnull, IEquatable<TKey>
{
    private readonly CacheOptions cacheOptions;

    private readonly IDataProviderAsync<TKey, TValue> provider;
    private readonly InMemoryDataProvider<TKey, TValue> inMemoryDataProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="HybridCache{TKey, TValue}"/> class, which combines an in-memory cache 
    /// with a data provider for persistent storage. This class uses an eviction strategy to manage memory and a size calculator 
    /// to track cache item sizes.
    /// </summary>
    /// <param name="provider">The data provider responsible for interacting with the underlying data source for cache operations.
    /// This provider handles adding, retrieving, and removing cache items from a persistent store.</param>
    /// <param name="cacheOptions">The configuration options for the cache, including settings such as the maximum memory size 
    /// and default TTL (Time-To-Live) for cache entries.</param>
    /// <param name="evictionStrategy">The strategy used to manage cache eviction based on memory usage and access patterns.
    /// This strategy determines which items to remove when the in-memory cache exceeds its capacity.</param>
    /// <param name="sizeCalculator">A function that calculates the memory size of each cache item. This function is used by the
    /// in-memory cache to track and manage the total cache size, ensuring that the cache does not exceed the specified memory limits.</param>
    public HybridCache(IDataProviderAsync<TKey, TValue> provider, CacheOptions cacheOptions, IEvictionStrategy<TKey> evictionStrategy, Func<TValue, long> sizeCalculator)
    {
        this.provider = provider;
        this.cacheOptions = cacheOptions;
        this.inMemoryDataProvider = new InMemoryDataProvider<TKey, TValue>(evictionStrategy, sizeCalculator);
    }

    /// <inheritdoc />
    public async Task<TValue> GetAsync(TKey key)
    {
        var cacheItem = await this.inMemoryDataProvider.GetAsync(key);
        if (cacheItem != null)
        {
            return cacheItem;
        }

        return await this.provider.GetAsync(key);
    }

    /// <inheritdoc />
    public async Task<TValue> GetOrSetAsync(TKey key, Func<Task<TValue>> valueFactory, TimeSpan? ttl)
    {
        var item = await this.GetAsync(key);
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
        await this.inMemoryDataProvider.AddAsync(key, value, ttl.HasValue ? ttl : this.cacheOptions.DefaultTTL);
        await this.provider.AddAsync(key, value, ttl.HasValue ? ttl : this.cacheOptions.DefaultTTL);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(TKey key)
    {
        await this.inMemoryDataProvider.RemoveAsync(key);
        await this.provider.RemoveAsync(key);
    }
}
