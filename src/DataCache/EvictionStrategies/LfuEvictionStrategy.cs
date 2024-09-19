using DataCache.Abstraction;
using DataCache.Configurations;

namespace DataCache.EvictionStrategies;

/// <summary>
/// This strategy tracks the frequency of access to determine which key is the least frequently used.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify cache items. Must implement <see cref="IEquatable{TKey}"/> and cannot be null.</typeparam>
public class LfuEvictionStrategy<TKey> : IEvictionStrategy<TKey>
    where TKey : notnull, IEquatable<TKey>
{
    private readonly Dictionary<TKey, int> accessCounts = new ();
    private readonly SortedDictionary<int, HashSet<TKey>> frequencyMap = new ();
    private readonly object @lock = new ();
    private readonly long maxSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="LfuEvictionStrategy{TKey}"/> class.
    /// </summary>
    /// <param name="cacheOptions">The configuration options for the cache, including settings such as the maximum memory size and default TTL (Time-To-Live) for cache entries.</param>
    public LfuEvictionStrategy(CacheOptions cacheOptions)
    {
        this.maxSize = cacheOptions.MaxMemorySize;
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
            // Initialize frequency to 1 for new items
            this.accessCounts[key] = 1;
            if (!this.frequencyMap.TryGetValue(1, out var keysAtFrequencyOne))
            {
                keysAtFrequencyOne = new HashSet<TKey>();
                this.frequencyMap[1] = keysAtFrequencyOne;
            }

            keysAtFrequencyOne.Add(key);
        }
    }

    /// <inheritdoc />
    public void OnItemAccessed(TKey key)
    {
        lock (this.@lock)
        {
            if (this.accessCounts.TryGetValue(key, out var oldFrequency))
            {
                // Update frequency map by removing from old frequency and adding to new frequency
                this.frequencyMap[oldFrequency].Remove(key);
                if (this.frequencyMap[oldFrequency].Count == 0)
                {
                    this.frequencyMap.Remove(oldFrequency);
                }

                var newFrequency = oldFrequency + 1;
                this.accessCounts[key] = newFrequency;

                if (!this.frequencyMap.TryGetValue(newFrequency, out var keysAtNewFrequency))
                {
                    keysAtNewFrequency = new HashSet<TKey>();
                    this.frequencyMap[newFrequency] = keysAtNewFrequency;
                }

                keysAtNewFrequency.Add(key);
            }
        }
    }

    /// <inheritdoc />
    public void OnItemRemoved(TKey key, long size)
    {
        lock (this.@lock)
        {
            if (this.accessCounts.TryGetValue(key, out var frequency))
            {
                // Remove the key from the frequency map and access counts
                this.frequencyMap[frequency].Remove(key);
                if (this.frequencyMap[frequency].Count == 0)
                {
                    this.frequencyMap.Remove(frequency);
                }

                this.accessCounts.Remove(key);
                this.CurrentSize -= size;
            }
        }
    }

    /// <inheritdoc />
    public TKey GetEvictionKey()
    {
        lock (this.@lock)
        {
            if (this.frequencyMap.Count > 0)
            {
                // Get the set of items with the least frequency (first entry in SortedDictionary)
                var leastFrequent = this.frequencyMap.First();
                var key = leastFrequent.Value.First();

                // Remove the evicted key from the frequency map and access counts
                leastFrequent.Value.Remove(key);
                if (leastFrequent.Value.Count == 0)
                {
                    this.frequencyMap.Remove(leastFrequent.Key);
                }

                this.accessCounts.Remove(key);

                return key;
            }
        }

        return default!;
    }
}
