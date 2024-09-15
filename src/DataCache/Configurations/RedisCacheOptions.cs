using DataCache.Cache;

namespace DataCache.Configurations;

/// <summary>
/// Represents configuration options specific to the Redis cache, extending the base cache options
/// with additional Redis-related settings.
/// </summary>
public class RedisCacheOptions : CacheOptions
{
    /// <summary>
    /// Gets the index of the Redis database to be used. 
    /// This is useful for selecting which Redis database instance to interact with, especially when
    /// using a Redis server with multiple databases.
    /// </summary>
    public int DatabaseIndex { get; init; }

    /// <summary>
    /// Gets a value indicating whether in-memory caching is enabled for optimization. 
    /// When set to <c>true</c>, the cache will use an in-memory cache to reduce the number of trips
    /// to the Redis database, improving performance by caching frequently accessed data in memory.
    /// </summary>
    public bool Optimized { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCacheOptions"/> class with the specified
    /// maximum memory size, eviction strategy, TTL interval, Redis database index, and optimization flag.
    /// </summary>
    /// <param name="maxMemorySize">The maximum allowed memory size for the cache, in bytes.</param>
    /// <param name="evictionType">The eviction strategy to be used by the cache.</param>
    /// <param name="TtlInterval">The time interval between checks for expired cache items.</param>
    /// <param name="databaseIndex">The index of the Redis database to be used.</param>
    /// <param name="optimized">A value indicating whether in-memory caching is enabled for optimization.</param>
    public RedisCacheOptions(long maxMemorySize, Eviction evictionType, TimeSpan TtlInterval, int databaseIndex, bool optimized)
        : base(maxMemorySize, evictionType, TtlInterval)
    {
        if (databaseIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(maxMemorySize), "DatabaseIndex cannot be negative");

        DatabaseIndex = databaseIndex;
        Optimized = optimized;
    }
}
