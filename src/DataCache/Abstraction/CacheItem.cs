
namespace DataCache.Abstraction;

public record CacheItem<TValue>(TValue Value, DateTimeOffset CreatedAt, DateTimeOffset? ExpiredAt);
