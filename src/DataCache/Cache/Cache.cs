using System.Collections.Concurrent;

namespace DataCache.Cache;


public class Cache<TValue> : IDisposable
{
    private readonly ConcurrentDictionary<string, CacheItem<TValue>> _cache = new();
    private readonly Task _cleanUpTask;   
    public Cache()
    {
        _cleanUpTask = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000);
                RemoveExpiredItems();
            }

        });
    }

    public void Set(string key, TValue value, TimeSpan ttl)
    {
        if (key is not null)
        {
            var expiration = DateTime.Now.Add(ttl);
            var cacheItem = new CacheItem<TValue>(value, expiration);
            _cache.AddOrUpdate(key, cacheItem, (oldKey, oldValue) => cacheItem);
        }
    }

    public TValue? Get(string key)
    {
        if (key is not null && _cache.TryGetValue(key, out var item))
        {
            if (!item.IsExpired)
            {
                return item.Value;
            }
            Remove(key);
        }
        return default;

    }

    public void Remove(string key)
    {
        if (key is not null)
        {
            _cache.TryRemove(key, out _);
        }
    }
    public void Dispose()
    {
        _cache.Clear();
        _cleanUpTask.Dispose();     
    }

    private void RemoveExpiredItems()
    {
        foreach (var key in _cache.Keys)
        {
            if (_cache.TryGetValue(key, out var item) && item.IsExpired)
            {
                Remove(key);
            }
        }
    }
}
