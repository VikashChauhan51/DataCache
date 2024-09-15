using DataCache.Configurations;
using StackExchange.Redis;
using System.Text.Json;

namespace DataCache.Cache;

public class RedisCache : CacheBase, ICacheAsync
{
    private readonly IDatabase _redisDatabase;
    private readonly RedisCacheOptions _options;
    private readonly MemoryCache _inMemoryCache;
    private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Gets the count of items in the in-memory cache.
    /// </summary>
    public long Count => _inMemoryCache.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCache"/> class.
    /// </summary>
    /// <param name="options">The cache options.</param>
    /// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    public RedisCache(RedisCacheOptions options, IConnectionMultiplexer connectionMultiplexer) : base(options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _redisDatabase = connectionMultiplexer.GetDatabase(_options.DatabaseIndex);
        _inMemoryCache = new MemoryCache(options);
    }

    /// <summary>
    /// Asynchronously gets a cache item by key.
    /// </summary>
    /// <param name="key">The key of the item to get.</param>
    /// <returns>The cache item, or <c>null</c> if not found.</returns>
    public async Task<CacheItem?> GetAsync(string key)
    {
        await _cacheLock.WaitAsync();
        try
        {
            if (_options.Optimized)
            {
                var cacheItem = await _inMemoryCache.GetAsync(key);
                if (cacheItem != null)
                {
                    return cacheItem;
                }
            }

            var value = await _redisDatabase.StringGetAsync(key);
            if (value.HasValue)
            {
                var item = JsonSerializer.Deserialize<CacheItem>(value);
                if (_options.Optimized && item != null)
                {
                    // Cache in memory if optimized
                    await _inMemoryCache.PutAsync(key, item);

                    // Update TTL if present
                    if (item.Ttl.HasValue)
                    {
                        await _redisDatabase.KeyExpireAsync(key, item.Ttl.Value);
                    }
                }
                return item;
            }
            return null;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Asynchronously puts a cache item by key.
    /// </summary>
    /// <param name="key">The key of the item to put.</param>
    /// <param name="item">The cache item to put.</param>
    public async Task PutAsync(string key, CacheItem item)
    {
        await _cacheLock.WaitAsync();
        try
        {
            item = item ?? new CacheItem(default!, DateTime.UtcNow, default);

            if (_options.Optimized)
            {
                // Add to in-memory cache
                await _inMemoryCache.PutAsync(key, item);
            }

            // Serialize item once
            var serializedItem = JsonSerializer.Serialize(item);
            var ttl = item.Ttl.HasValue ? (TimeSpan?)item.Ttl.Value : null;

            // Set cache in Redis with TTL
            await _redisDatabase.StringSetAsync(key, serializedItem, ttl);
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Asynchronously deletes a cache item by key.
    /// </summary>
    /// <param name="key">The key of the item to delete.</param>
    public async Task DeleteAsync(string key)
    {
        await _cacheLock.WaitAsync();
        try
        {
            await _inMemoryCache.DeleteAsync(key);
            await _redisDatabase.KeyDeleteAsync(key);
        }
        finally
        {
            _cacheLock.Release();
        }
    }
}
