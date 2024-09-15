using DataCache.Configurations;
using DataCache.EvictionStrategies;
using System.Collections.Concurrent;

namespace DataCache.Cache;


/// <summary>
/// Base class for cache implementations, providing common functionality for managing cache items,
/// eviction strategies, and memory size calculations.
/// </summary>
public abstract class CacheBase
{
    protected static readonly ConcurrentDictionary<string, CacheItem> _cacheMap = new();
    private static readonly IReadOnlyDictionary<Eviction, Func<IEvictionStrategyAsync<string>>> _strategyMap =
        new Dictionary<Eviction, Func<IEvictionStrategyAsync<string>>>
        {
            { Eviction.LRU, () => new LruEvictionStrategy() },
            { Eviction.LFU, () => new LfuEvictionStrategy() },
            { Eviction.MRU, () => new MruEvictionStrategy() },
            { Eviction.RoundRobin, () => new RoundRobinEvictionStrategy() },
            { Eviction.None, () => new DefaultEvictionStrategy() }
        };

    protected readonly CacheOptions _cacheOptions;
    protected readonly IEvictionStrategyAsync<string> _evictionStrategy;
    protected long _currentMemorySize;
    private readonly SemaphoreSlim _cacheSemaphore = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheBase"/> class with the specified cache options.
    /// </summary>
    /// <param name="cacheOptions">The configuration options for the cache.</param>
    protected CacheBase(CacheOptions cacheOptions)
    {
        _cacheOptions = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
        _evictionStrategy = _strategyMap[_cacheOptions.EvictionType]();
        _currentMemorySize = 0;
    }

    /// <summary>
    /// Evicts cache items to make room for a new item of the specified size, based on the configured eviction strategy.
    /// </summary>
    /// <param name="valueSize">The size of the new item to be added to the cache.</param>
    protected virtual async Task EvictItemsToFit(long valueSize)
    {
        if (_cacheOptions.EvictionType == Eviction.None ||
            _cacheOptions.MaxMemorySize <= 0 ||
            _currentMemorySize + valueSize <= _cacheOptions.MaxMemorySize)
        {
            return;
        }

        await _cacheSemaphore.WaitAsync();

        try
        {
            while (_currentMemorySize + valueSize > _cacheOptions.MaxMemorySize)
            {
                var evictKey = await _evictionStrategy.EvictItemAsync();
                if (_cacheMap.TryRemove(evictKey, out var evictedItem))
                {
                    _currentMemorySize -= CalculateCacheItemSize(evictedItem);
                }
            }
        }
        finally
        {
            _cacheSemaphore.Release();
        }
    }

    /// <summary>
    /// Calculates the size of the specified cache item in bytes.
    /// </summary>
    /// <param name="item">The cache item for which to calculate the size.</param>
    /// <returns>The size of the cache item in bytes.</returns>
    protected long CalculateCacheItemSize(CacheItem item)
    {
        if (item == null)
        {
            return 0;
        }

        // Calculate size of the string (each char is 2 bytes in .NET)
        long dataSize = (item.Value?.Length ?? 0) * sizeof(char);
        // Add size of DateTimeOffset (16 bytes in .NET) and 4 bytes for TimeSpan
        dataSize += 20;

        return dataSize;
    }
}


