namespace DataCache.Configurations;

/// <summary>
/// Represents configuration options for the cache system, including memory size limits and TTL (Time-To-Live) interval settings.
/// This class defines the maximum memory usage and the default time-to-live for cache items.
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheOptions"/> class.
    /// </summary>
    /// <param name="maxMemorySize">The maximum memory size (in bytes) allowed for the cache. Must be non-negative.</param>
    /// <param name="ttlInterval">The default TTL interval for cache items. This defines how long cache items should live before expiration.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxMemorySize"/> is negative or <paramref name="ttlInterval"/> is negative.</exception>
    public CacheOptions(long maxMemorySize, TimeSpan ttlInterval)
    {
        if (maxMemorySize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxMemorySize), "MaxMemorySize cannot be negative");
        }

        if (ttlInterval < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ttlInterval), "TTL interval cannot be negative");
        }

        this.MaxMemorySize = maxMemorySize;
        this.DefaultTTL = ttlInterval;
    }

    /// <summary>
    /// Gets the maximum allowed memory size for the cache, in bytes.
    /// This defines the limit on the amount of memory that can be used by the cache before eviction policies are triggered.
    /// </summary>
    public long MaxMemorySize { get; init; }

    /// <summary>
    /// Gets the default time-to-live (TTL) interval for cache items.
    /// If an item is added without specifying a TTL, this default value will be used to determine how long the item should remain in the cache.
    /// </summary>
    public TimeSpan DefaultTTL { get; init; }
}