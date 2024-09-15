using DataCache.Configurations;
using DataCache.EvictionStrategies;
using System.Collections.Concurrent;

namespace DataCache.Cache;

public abstract class CacheBase
{
    protected static readonly ConcurrentDictionary<string, CacheItem> _cacheMap = new();
    protected readonly CacheOptions _cacheOptions;
    protected IEvictionStrategy<string> _evictionStrategy;
    protected long _currentMemorySize;
    // Map of eviction strategies
    private static readonly IReadOnlyDictionary<Eviction, Func<IEvictionStrategy<string>>> _strategyMap =
        new Dictionary<Eviction, Func<IEvictionStrategy<string>>>
        {
            { Eviction.LRU, () => new LruEvictionStrategy() },
            { Eviction.LFU, () => new LfuEvictionStrategy() },
            { Eviction.MRU, () => new MruEvictionStrategy() },
            { Eviction.None, () => new DefaultEvictionStrategy() }
        };

     
    protected CacheBase(CacheOptions cacheOptions)
    {
        _cacheOptions = cacheOptions ?? new(0, Eviction.None, TimeSpan.Zero);
        _evictionStrategy = _strategyMap[_cacheOptions.EvictionType]();
        _currentMemorySize = 0;
    }

     
    protected void EvictItemsToFit(long valueSize)
    {
        if (_cacheOptions.EvictionType != Eviction.None && _currentMemorySize + valueSize > _cacheOptions.MaxMemorySize)
        {
            while (_currentMemorySize + valueSize > _cacheOptions.MaxMemorySize)
            {
                var evictKey = _evictionStrategy.EvictItem();
                var evictedItem = _cacheMap[evictKey];
                _cacheMap.TryRemove(evictKey,out var _);
                _currentMemorySize -= CalculateCacheItemSize(evictedItem);
            }
        }
    }
    protected long CalculateCacheItemSize(CacheItem item)
    {
        if (item != null)
        {
            // Calculate size of the string (each char is 2 bytes in .NET)
            long dataSize = (item.Value?.Length ?? 0) * sizeof(char);
            // Add size of DateTimeOffset (16 bytes in .NET) and 4 bytes for TimeSpan
            dataSize += 20;

            return dataSize;
        }

        return 0;
    }
}
