using DataCache.Abstraction;
using DataCache.Configurations;

namespace DataCache.EvictionStrategies;

/// <summary>
/// This strategy tracks the order of keys to determine which one is the most recently used.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify cache items. Must implement <see cref="IEquatable{TKey}"/> and cannot be null.</typeparam>
public class MruEvictionStrategy<TKey> : EvictionStrategyBase, IEvictionStrategy<TKey>
    where TKey : notnull, IEquatable<TKey>
{
    private readonly LinkedList<TKey> accessOrder = new ();
    private readonly Dictionary<TKey, LinkedListNode<TKey>> cacheMap = new ();
    private readonly object @lock = new ();

    /// <summary>
    /// Initializes a new instance of the <see cref="MruEvictionStrategy{TKey}"/> class.
    /// </summary>
    /// <param name="cacheOptions">The configuration options for the cache, including settings such as the maximum memory size and default TTL (Time-To-Live) for cache entries.</param>
    public MruEvictionStrategy(CacheOptions cacheOptions)
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
        lock (this.@lock)
        {
            if (!this.cacheMap.ContainsKey(key))
            {
                var node = new LinkedListNode<TKey>(key);
                this.accessOrder.AddFirst(node);
                this.cacheMap[key] = node;
            }
        }
    }

    /// <inheritdoc />
    public void OnItemAccessed(TKey key)
    {
        lock (this.@lock)
        {
            if (this.cacheMap.TryGetValue(key, out var node))
            {
                this.accessOrder.Remove(node);
                this.accessOrder.AddFirst(node);
            }
        }
    }

    /// <inheritdoc />
    public void OnItemRemoved(TKey key, long size)
    {
        lock (this.@lock)
        {
            if (this.cacheMap.TryGetValue(key, out var node))
            {
                this.accessOrder.Remove(node);
                this.cacheMap.Remove(key);
                this.CurrentSize -= size;
            }
        }
    }

    /// <inheritdoc />
    public TKey GetEvictionKey()
    {
        lock (this.@lock)
        {
            var mostRecent = this.accessOrder.First;
            if (mostRecent != null)
            {
                this.accessOrder.RemoveFirst();
                this.cacheMap.Remove(mostRecent.Value);
                return mostRecent.Value;
            }
        }

        return default!;
    }
}
