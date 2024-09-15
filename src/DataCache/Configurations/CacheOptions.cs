using DataCache.Cache;

namespace DataCache.Configurations;

/// <summary>
/// Represents configuration options for the cache system, including memory size limits,
/// eviction strategies, and TTL (Time-To-Live) interval settings.
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Gets the maximum allowed memory size for the cache, in bytes. 
    /// This defines the limit on the amount of memory that can be used by the cache.
    /// </summary>
    public long MaxMemorySize { get; init; }

    /// <summary>
    /// Gets the eviction strategy to be used by the cache. 
    /// Determines how the cache will decide which items to remove when the cache reaches its limit.
    /// Possible values include:
    /// - <see cref="Eviction.None"/>: No eviction policy.
    /// - <see cref="Eviction.LRU"/>: Least Recently Used.
    /// - <see cref="Eviction.LFU"/>: Least Frequently Used.
    /// - <see cref="Eviction.MRU"/>: Most Recently Used.
    /// - <see cref="Eviction.RoundRobin"/>: Round-Robin eviction.
    /// </summary>
    public Eviction EvictionType { get; init; }

    /// <summary>
    /// Gets the time interval between checks for expired cache items. 
    /// This interval defines how often the cache should perform cleanup operations to 
    /// remove items that have exceeded their TTL (Time-To-Live).
    /// </summary>
    public TimeSpan TtlInterval { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheOptions"/> class with the specified
    /// maximum memory size, eviction strategy, and TTL interval.
    /// </summary>
    /// <param name="maxMemorySize">The maximum allowed memory size for the cache, in bytes.</param>
    /// <param name="evictionType">The eviction strategy to be used by the cache.</param>
    /// <param name="ttlInterval">The time interval between checks for expired cache items.</param>
    public CacheOptions(long maxMemorySize, Eviction evictionType, TimeSpan ttlInterval)
    {
        if (maxMemorySize < 0)
            throw new ArgumentOutOfRangeException(nameof(maxMemorySize), "MaxMemorySize cannot be negative");

        if (!Enum.IsDefined(typeof(Eviction), evictionType))
            throw new ArgumentException("Invalid eviction type", nameof(evictionType));

        if (ttlInterval < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(ttlInterval), "TTL interval cannot be negative");


        MaxMemorySize = maxMemorySize;
        EvictionType = evictionType;
        TtlInterval = ttlInterval;
    }
}


