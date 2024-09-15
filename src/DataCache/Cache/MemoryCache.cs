using DataCache.Configurations;

namespace DataCache.Cache;


/// <summary>
/// A memory cache implementation that supports asynchronous operations and eviction strategies.
/// </summary>
public class MemoryCache : CacheBase, ICacheAsync
{
    private  readonly TimeSpan _minCleanupDelay = TimeSpan.FromMinutes(1);
    private readonly object _cleanupLock = new();
    private readonly CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryCache"/> class with the specified cache options.
    /// </summary>
    /// <param name="cacheOptions">The configuration options for the cache.</param>
    public MemoryCache(CacheOptions cacheOptions) : base(cacheOptions)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        StartCleanupTask();
    }

    /// <summary>
    /// Asynchronously retrieves the cache item associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the cache item to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the cache item if found, otherwise null.</returns>
    public async Task<CacheItem?> GetAsync(string key)
    {
        if (_cacheMap.TryGetValue(key, out var cacheItem))
        {
            await _evictionStrategy.AccessItemAsync(key);
            return cacheItem;
        }
        return null;
    }

    /// <summary>
    /// Asynchronously adds or updates a cache item with the specified key.
    /// </summary>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="item">The cache item to add or update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task PutAsync(string key, CacheItem item)
    {
        var cacheItem = item ?? new CacheItem(default!, DateTimeOffset.UtcNow, default);
        long valueSize = CalculateCacheItemSize(cacheItem);

        await EvictItemsToFit(valueSize);

        if (_cacheMap.ContainsKey(key))
        {
            var oldItem = _cacheMap[key];
            _currentMemorySize -= CalculateCacheItemSize(oldItem);
            _cacheMap[key] = cacheItem;
        }
        else
        {
            _cacheMap.TryAdd(key, cacheItem);
            await _evictionStrategy.AddItemAsync(key);
        }

        _currentMemorySize += valueSize;
    }

    /// <summary>
    /// Asynchronously deletes the cache item associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the cache item to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteAsync(string key)
    {
        if (_cacheMap.TryRemove(key, out var cacheItem))
        {
            await _evictionStrategy.RemoveItemAsync(key);
            _currentMemorySize -= CalculateCacheItemSize(cacheItem);
        }
    }

    /// <summary>
    /// Gets the number of items currently in the cache.
    /// </summary>
    public long Count => _cacheMap.Count;

    /// <summary>
    /// Starts a background task that periodically cleans up expired cache items.
    /// </summary>
    private void StartCleanupTask()
    {     
        var token = _cancellationTokenSource.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var delay = _cacheOptions.TtlInterval > _minCleanupDelay ? _cacheOptions.TtlInterval : _minCleanupDelay;
                    await Task.Delay(delay, token);

                    if (!token.IsCancellationRequested)
                    {
                        await CleanUpExpiredItems();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Handle task cancellation
            }
            catch { }
        }, token);
    }


    /// <summary>
    /// Cancels the background cleanup task and releases resources.
    /// </summary>
    public void StopCleanupTask()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

    /// <summary>
    /// Asynchronously removes expired cache items based on their TTL (time-to-live).
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task CleanUpExpiredItems()
    {
        var now = DateTimeOffset.UtcNow;
        var keysToRemove = new List<string>();

        lock (_cleanupLock)
        {
            keysToRemove.AddRange(_cacheMap
                .Where(x => x.Value.Ttl.HasValue && x.Value.CreatedAt.Add(x.Value.Ttl.Value) < now)
                .Select(x => x.Key));
        }

        foreach (var key in keysToRemove)
        {
            await DeleteAsync(key);
        }
    }
}
