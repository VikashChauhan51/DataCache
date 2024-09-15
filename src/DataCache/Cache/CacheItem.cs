namespace DataCache.Cache;

public record CacheItem(string Value, DateTimeOffset CreatedAt, TimeSpan? Ttl);

