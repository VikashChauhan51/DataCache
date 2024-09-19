using System.Collections.Concurrent;
using DataCache.Abstraction;

namespace DataCache.DataProviders;

/// <summary>
/// In-memory cache data provider that stores cache items in memory and supports an eviction strategy for managing memory usage.
/// This class manages the lifecycle of cached items, including adding, retrieving, and removing items from the in-memory cache.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify cache items. Must implement <see cref="IEquatable{T}"/> and not be null.</typeparam>
/// <typeparam name="TValue">The type of the value stored in the cache.</typeparam>
public class InMemoryDataProvider<TKey, TValue> : IDataProviderAsync<TKey, TValue>
    where TKey : notnull, IEquatable<TKey>
{
    private static readonly ConcurrentDictionary<TKey, CacheItem<TValue>> CacheMap = new ();

    private readonly IEvictionStrategy<TKey> evictionStrategy;
    private readonly Func<TValue, long> sizeCalculator;
    private readonly TimeSpan minCleanupDelay = TimeSpan.FromMinutes(1);
    private readonly object cleanupLock = new ();
    private readonly CancellationTokenSource cancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryDataProvider{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="evictionStrategy">The eviction strategy used to manage cache eviction when memory limits are reached. 
    /// This strategy determines which items to remove when the cache exceeds the maximum size.</param>
    /// <param name="sizeCalculator">A function to calculate the memory size of each cache item. 
    /// This is used to track the total size of the in-memory cache and enforce size limits.</param>
    public InMemoryDataProvider(IEvictionStrategy<TKey> evictionStrategy, Func<TValue, long> sizeCalculator)
    {
        this.evictionStrategy = evictionStrategy;
        this.sizeCalculator = sizeCalculator;
        this.cancellationTokenSource = new CancellationTokenSource();
        this.StartCleanupTask();
    }

    /// <inheritdoc />
    public async Task AddAsync(TKey key, TValue value, TimeSpan? ttl)
    {
        var size = this.sizeCalculator(value);
        while (this.evictionStrategy.CurrentSize + size > this.evictionStrategy.MaxSize)
        {
            var evictKey = this.evictionStrategy.GetEvictionKey();
            await this.RemoveAsync(evictKey);
        }

        DateTimeOffset? expirtedTime = ttl.HasValue ? DateTimeOffset.UtcNow.Add(ttl.Value) : null;
        var cacheItem = new CacheItem<TValue>(value, DateTimeOffset.UtcNow, expirtedTime);
        CacheMap[key] = cacheItem;
        this.evictionStrategy.OnItemAdded(key);
    }

    /// <inheritdoc />
    public Task<TValue> GetAsync(TKey key)
    {
        if (CacheMap.TryGetValue(key, out var cacheItem))
        {
            if (cacheItem.ExpiredAt > DateTimeOffset.UtcNow)
            {
                this.evictionStrategy.OnItemAccessed(key);
                return Task.FromResult<TValue>(cacheItem.Value);
            }

            // remove in background thread.
            this.RemoveAsync(key).ConfigureAwait(false);
        }

        return default!;
    }

    /// <inheritdoc />
    public Task RemoveAsync(TKey key)
    {
        if (CacheMap.TryRemove(key, out var cacheItem))
        {
            var size = this.sizeCalculator(cacheItem.Value);
            this.evictionStrategy.OnItemRemoved(key, size);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Cancels the background cleanup task and releases resources.
    /// </summary>
    public void StopCleanupTask()
    {
        this.cancellationTokenSource?.Cancel();
        this.cancellationTokenSource?.Dispose();
    }

    /// <summary>
    /// start cleaning.
    /// </summary>
    private void StartCleanupTask()
    {
        var token = this.cancellationTokenSource.Token;

        _ = Task.Run(
            async () =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(this.minCleanupDelay, token);

                    if (!token.IsCancellationRequested)
                    {
                        await this.CleanUpExpiredItems();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Handle task cancellation
            }
            catch (Exception)
            {
                // Handle it later
            }
        }, token);
    }

    /// <summary>
    /// Asynchronously removes expired cache items based on their TTL (time-to-live).
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task CleanUpExpiredItems()
    {
        var now = DateTimeOffset.UtcNow;
        var keysToRemove = new List<TKey>();

        lock (this.cleanupLock)
        {
            keysToRemove.AddRange(CacheMap
                .Where(x => x.Value.ExpiredAt.HasValue && x.Value.ExpiredAt < now)
                .Select(x => x.Key));
        }

        foreach (TKey key in keysToRemove)
        {
            await this.RemoveAsync(key);
        }
    }
}