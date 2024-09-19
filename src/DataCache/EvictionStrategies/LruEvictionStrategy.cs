using System.Collections.Concurrent;
using DataCache.Abstraction;
using DataCache.Configurations;

namespace DataCache.EvictionStrategies;

/// <summary>
/// The LRU (Least Recently Used) strategy evicts the least recently accessed items.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify cache items. Must implement <see cref="IEquatable{TKey}"/> and cannot be null.</typeparam>
public class LruEvictionStrategy<TKey> : EvictionStrategyBase, IEvictionStrategy<TKey>
    where TKey : notnull, IEquatable<TKey>
{
    private readonly LinkedList<TKey> accessOrder = new ();
    private readonly ConcurrentDictionary<TKey, LinkedListNode<TKey>> keyNodes = new ();
    private readonly object @lock = new ();

    /// <summary>
    /// Initializes a new instance of the <see cref="LruEvictionStrategy{TKey}"/> class.
    /// </summary>
    /// <param name="cacheOptions">The configuration options for the cache, including settings such as the maximum memory size and default TTL (Time-To-Live) for cache entries.</param>
    public LruEvictionStrategy(CacheOptions cacheOptions)
        : base(cacheOptions)
    {
    }

    /// <inheritdoc />
    public long MaxSize => this.maxSize;

    /// <inheritdoc />
    public long CurrentSize { get; private set; }

    /// <inheritdoc />
    public void OnItemAdded(TKey key)
    {
        var node = new LinkedListNode<TKey>(key);
        if (this.keyNodes.TryAdd(key, node))
        {
            lock (this.@lock)
            {
                this.accessOrder.AddLast(node);
            }
        }
    }

    /// <inheritdoc />
    public void OnItemAccessed(TKey key)
    {
        if (this.keyNodes.TryGetValue(key, out var node))
        {
            lock (this.@lock)
            {
                this.accessOrder.Remove(node);
                this.accessOrder.AddLast(node);
            }
        }
    }

    /// <inheritdoc />
    public void OnItemRemoved(TKey key, long size)
    {
        if (this.keyNodes.TryRemove(key, out var node))
        {
            lock (this.@lock)
            {
                this.accessOrder.Remove(node);
            }

            this.CurrentSize -= size;
        }
    }

    /// <inheritdoc />
    public TKey GetEvictionKey()
    {
        lock (this.@lock)
        {
            var oldest = this.accessOrder.First;
            if (oldest != null)
            {
                this.accessOrder.RemoveFirst();
                this.keyNodes.TryRemove(oldest.Value, out _);
                return oldest.Value;
            }
        }

        return default!;
    }
}