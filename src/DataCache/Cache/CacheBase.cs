using DataCache.Configurations;

namespace DataCache.Cache;

/// <summary>
///  The base class of cache implementations.
/// </summary>
public abstract class CacheBase
{
    private readonly CacheOptions cacheOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheBase"/> class, which provides a caching mechanism
    /// with customizable options and a data provider for managing cache entries.
    /// </summary>
    /// <param name="cacheOptions">The configuration options for the cache, including settings such as the maximum memory size 
    /// and default TTL (Time-To-Live) for cache entries.</param>
    protected CacheBase(CacheOptions cacheOptions)
    {
        this.cacheOptions = cacheOptions;
    }

    /// <summary>
    /// Get default time to alive value if configured.
    /// </summary>
    /// <returns>Return timespan value if configured else null.</returns>
    protected TimeSpan? GetDefaultTimeToAlive()
    {
        return this.cacheOptions.DefaultTTL > TimeSpan.Zero ? this.cacheOptions.DefaultTTL : null;
    }
}
