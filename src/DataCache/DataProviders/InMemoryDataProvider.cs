using DataCache.Abstraction;
using System.Collections.Concurrent;


namespace DataCache.DataProviders;

public class InMemoryDataProvider<TKey, TValue> : IDataProviderAsync<TKey, TValue> where TKey : notnull, IEquatable<TKey>
{
    protected static readonly ConcurrentDictionary<TKey, CacheItem<TValue>> _cacheMap = new();

    private readonly IEvictionStrategy<TKey> _evictionStrategy;
    private readonly Func<TValue, long> _sizeCalculator;
    private readonly TimeSpan _minCleanupDelay = TimeSpan.FromMinutes(1);
    private readonly object _cleanupLock = new();
    private readonly CancellationTokenSource _cancellationTokenSource;
    public InMemoryDataProvider(IEvictionStrategy<TKey> evictionStrategy, Func<TValue, long> sizeCalculator)
    {
        _evictionStrategy = evictionStrategy;
        _sizeCalculator = sizeCalculator;
        _cancellationTokenSource = new CancellationTokenSource();
        StartCleanupTask();
    }

    public async Task AddAsync(TKey key, TValue value, TimeSpan? ttl)
    {
        var size = _sizeCalculator(value);
        while (_evictionStrategy.CurrentSize + size > _evictionStrategy.MaxSize)
        {
            var evictKey = _evictionStrategy.GetEvictionKey();
            await RemoveAsync(evictKey);
        }

        DateTimeOffset? expirtedTime = ttl.HasValue ? DateTimeOffset.UtcNow.Add(ttl.Value) : null;
        var cacheItem = new CacheItem<TValue>(value, DateTimeOffset.UtcNow, expirtedTime);
        _cacheMap[key] = cacheItem;
        _evictionStrategy.OnItemAdded(key);
    }

    public Task<TValue> GetAsync(TKey key)
    {
        if (_cacheMap.TryGetValue(key, out var cacheItem))
        {
            if (cacheItem.ExpiredAt > DateTimeOffset.UtcNow)
            {
                _evictionStrategy.OnItemAccessed(key);
                return Task.FromResult<TValue>(cacheItem.Value);
            }
            // remove in background thread.
            RemoveAsync(key).ConfigureAwait(false);
        }
        return default!;
    }

    public Task RemoveAsync(TKey key)
    {
        if (_cacheMap.TryRemove(key, out var cacheItem))
        {
            var size = _sizeCalculator(cacheItem.Value);
            _evictionStrategy.OnItemRemoved(key, size);
        }
        return Task.CompletedTask;
    }



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
                    await Task.Delay(_minCleanupDelay, token);

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
        var keysToRemove = new List<TKey>();

        lock (_cleanupLock)
        {
            keysToRemove.AddRange(_cacheMap
                .Where(x => x.Value.ExpiredAt.HasValue && x.Value.ExpiredAt < now)
                .Select(x => x.Key));
        }

        foreach (TKey key in keysToRemove)
        {
            await RemoveAsync(key);
        }
    }

}
