using DataCache.Abstraction;
using DataCache.Configurations;

namespace DataCache.EvictionStrategies;

/// <summary>
/// This strategy evicts items in a round-robin fashion, cycling through all keys in order.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify cache items. Must implement <see cref="IEquatable{TKey}"/> and cannot be null.</typeparam>
public class RandomEvictionStrategy<TKey> : EvictionStrategyBase, IEvictionStrategy<TKey>
    where TKey : notnull, IEquatable<TKey>
{
    private readonly List<TKey> keys = new ();
    private readonly object @lock = new ();
    private int currentIndex = -1; // Tracks the current index for eviction

    /// <summary>
    /// Initializes a new instance of the <see cref="RandomEvictionStrategy{TKey}"/> class.
    /// </summary>
    /// <param name="cacheOptions">The configuration options for the cache, including settings such as the maximum memory size and default TTL (Time-To-Live) for cache entries.</param>
    public RandomEvictionStrategy(CacheOptions cacheOptions)
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
            // Add the new item to the list of keys
            if (!this.keys.Contains(key))
            {
                this.keys.Add(key);
            }
        }
    }

    /// <inheritdoc />
    public void OnItemAccessed(TKey key)
    {
        // Round-robin doesn't care about access patterns, so this is a no-op
    }

    /// <inheritdoc />
    public void OnItemRemoved(TKey key, long size)
    {
        lock (this.@lock)
        {
            if (this.keys.Remove(key))
            {
                // Adjust the current index if necessary
                if (this.keys.Count > 0 && this.currentIndex >= this.keys.Count)
                {
                    this.currentIndex = this.currentIndex % this.keys.Count;
                }
                else if (this.keys.Count == 0)
                {
                    this.currentIndex = -1;
                }

                this.CurrentSize -= size;
            }
        }
    }

    /// <inheritdoc />
    public TKey GetEvictionKey()
    {
        lock (this.@lock)
        {
            if (this.keys.Count == 0)
            {
                return default!; // No item to evict
            }

            // Move to the next item in round-robin fashion
            this.currentIndex = (this.currentIndex + 1) % this.keys.Count;
            var keyToEvict = this.keys[this.currentIndex];

            // Remove the key from the list
            this.keys.RemoveAt(this.currentIndex);

            // Adjust the current index, as the list size is reduced
            if (this.keys.Count > 0)
            {
                this.currentIndex = this.currentIndex % this.keys.Count;
            }
            else
            {
                this.currentIndex = -1; // Reset if no items are left
            }

            return keyToEvict;
        }
    }
}
