using DataCache.Configurations;

namespace DataCache.EvictionStrategies;

/// <summary>
///  The base class of Eviction Strategy implementations.
/// </summary>
public abstract class EvictionStrategyBase
{
    protected readonly long maxSize;
    private readonly CacheOptions cacheOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvictionStrategyBase"/> class, which provides a caching mechanism
    /// with customizable options and a data provider for managing cache entries.
    /// </summary>
    /// <param name="cacheOptions">The configuration options for the cache, including settings such as the maximum memory size 
    /// and default TTL (Time-To-Live) for cache entries.</param>
    protected EvictionStrategyBase(CacheOptions cacheOptions)
    {
        this.cacheOptions = cacheOptions;
        this.maxSize = cacheOptions.MaxMemorySize;
    }

    /// <summary>
    /// Memory size is configured.
    /// </summary>
    /// <returns>Return true if positive value configured else false.</returns>
    protected bool IsMemorySizeConfigured()
    {
        return this.cacheOptions.MaxMemorySize > 0;
    }
}
