using DataCache.Configurations;

namespace DataCache.Cache;

public class MemoryCache : CacheBase, ICache
{

    public MemoryCache(CacheOptions cacheOptions) : base(cacheOptions)
    {
    }

    public string Get(string key)
    {
        if (_cacheMap.TryGetValue(key, out var cacheItem))
        {
            _evictionStrategy.AccessItem(key);
            return cacheItem.Value;
        }

        throw new KeyNotFoundException("Key not found in cache");
    }

    public void Put(string key, string value)
    {
        var cacheItem = new CacheItem(value, DateTimeOffset.UtcNow);
        long valueSize = CalculateCacheItemSize(cacheItem);

        EvictItemsToFit(valueSize);


        if (_cacheMap.ContainsKey(key))
        {
            var oldItem = _cacheMap[key];
            _currentMemorySize -= CalculateCacheItemSize(oldItem);
            _cacheMap[key] = cacheItem;
        }
        else
        {
            _cacheMap.Add(key, cacheItem);
            _evictionStrategy.AddItem(key);
        }

        _currentMemorySize += valueSize;
    }

    public bool TryGet(string key, out string value)
    {
        if (_cacheMap.TryGetValue(key, out var cacheItem))
        {
            _evictionStrategy.AccessItem(key);
            value = cacheItem.Value;
            return true;
        }

        value = default!;
        return false;
    }

    public void Delete(string key)
    {
        if (_cacheMap.TryGetValue(key, out var cacheItem))
        {
            _evictionStrategy.RemoveItem(key);
        }

        throw new KeyNotFoundException("Key not found in cache");
    }

    public bool TryDelete(string key)
    {
        if (_cacheMap.TryGetValue(key, out var cacheItem))
        {
            _evictionStrategy.RemoveItem(key);
            return true;
        }
        return false;
    }

    public long Count => _cacheMap.Count;

}
