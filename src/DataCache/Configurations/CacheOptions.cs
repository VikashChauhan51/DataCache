using DataCache.Cache;

namespace DataCache.Configurations;

public class CacheOptions
{
    public long MaxMemorySize { get; init; }
    public Eviction EvictionType { get; init; }

    public CacheOptions(long maxMemorySize, Eviction evictionType)
    {
        MaxMemorySize = maxMemorySize;
        EvictionType = evictionType;
    }
}

