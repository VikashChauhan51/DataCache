using System.Collections.Concurrent;

namespace DataCache.Cache;


internal class Cache<TValue> : IDisposable
{
    private readonly ConcurrentDictionary<string, CacheItem<TValue>> _cache = new();
    private readonly Timer _cleanUpTimer;
    private readonly object _lockObj = new();
    public Cache()
    {
        _cleanUpTimer = new Timer(CleanUpCallBack, null, Timeout.Infinite, Timeout.Infinite);
        ScheduleNextCleanUp();
    }

    public void Set(string key, TValue value, TimeSpan ttl)
    {
        if (key is not null)
        {
            var expiration = DateTime.Now.Add(ttl);
            var cacheItem = new CacheItem<TValue>(value, expiration);
            _cache.AddOrUpdate(key, cacheItem, (oldKey, oldValue) => cacheItem);
            ScheduleNextCleanUp();
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
            ScheduleNextCleanUp();
        }
    }
    public void Dispose()
    {
        _cleanUpTimer.Dispose();
        _cache.Clear();
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

    private void CleanUpCallBack(object? state)
    {
        RemoveExpiredItems();
        ScheduleNextCleanUp();
    }

    private void ScheduleNextCleanUp()
    {
        lock (_lockObj)
        {

            var now = DateTime.Now;
            var nextExpiration = _cache.Values
                .Where(x => !x.IsExpired)
                .Select(x => x.Expiration)
                .OrderBy(x => x)
                .FirstOrDefault();

            if (nextExpiration != default)
            {
                var dueTime = nextExpiration - now;
                if (dueTime <= TimeSpan.Zero)
                {
                    dueTime = TimeSpan.FromMicroseconds(1);
                }
                _cleanUpTimer.Change(dueTime, Timeout.InfiniteTimeSpan);
            }
            else
            {
                _cleanUpTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
    }
}
