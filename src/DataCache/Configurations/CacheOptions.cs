using DataCache.Cache;

namespace DataCache.Configurations;

public class CacheOptions
{
    public long MaxMemorySize { get; init; }
    public Eviction EvictionType { get; init; }
    public TimeSpan Ttl { get; init; }

    public CacheOptions(long maxMemorySize, Eviction evictionType, TimeSpan ttl)
    {
        MaxMemorySize = maxMemorySize;
        EvictionType = evictionType;
        Ttl = ttl;
    }
}

