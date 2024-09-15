using DataCache.Cache;

namespace DataCache.Configurations;

public class RedisCacheOptions : CacheOptions
{
    public int DatabaseIndex { get; init; }
    public bool Optimized { get; init; }

    public RedisCacheOptions(long maxMemorySize, Eviction evictionType, TimeSpan ttl, int databaseIndex, bool optimized)
        : base(maxMemorySize, evictionType, ttl)
    {
        DatabaseIndex = databaseIndex;
        Optimized = optimized;
    }
}
