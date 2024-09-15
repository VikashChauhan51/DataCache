using DataCache.Cache;

namespace DataCache.Configurations;

public class RedisCacheOptions : CacheOptions
{
    public int DatabaseIndex { get; init; }
    public bool Optimized { get; init; }

    public RedisCacheOptions(long maxMemorySize, Eviction evictionType, int databaseIndex, bool optimized)
        : base(maxMemorySize, evictionType)
    {
        DatabaseIndex = databaseIndex;
        Optimized = optimized;
    }
}
