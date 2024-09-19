using DataCache.Cache;

namespace DataCache.Configurations;

/// <summary>
/// Represents configuration options for the cache system, including memory size limits and TTL (Time-To-Live) interval settings.
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Gets the maximum allowed memory size for the cache, in bytes. 
    /// This defines the limit on the amount of memory that can be used by the cache.
    /// </summary>
    public long MaxMemorySize { get; init; }

    public TimeSpan DefaultTTL { get; init; }

    public CacheOptions(long maxMemorySize, TimeSpan ttlInterval)
    {
        if (maxMemorySize < 0)
            throw new ArgumentOutOfRangeException(nameof(maxMemorySize), "MaxMemorySize cannot be negative");

        if (ttlInterval < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(ttlInterval), "TTL interval cannot be negative");

        MaxMemorySize = maxMemorySize;

        DefaultTTL = ttlInterval;
    }
}


