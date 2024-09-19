namespace DataCache.Abstraction;

/// <summary>
/// Represents a cache item that holds the value along with metadata about its creation and expiration times.
/// </summary>
/// <typeparam name="TValue">The type of the cached value.</typeparam>
/// <param name="Value">The cached value of type <typeparamref name="TValue"/>.</param>
/// <param name="CreatedAt">The <see cref="DateTimeOffset"/> indicating when the cache item was created.</param>
/// <param name="ExpiredAt">The <see cref="DateTimeOffset"/> indicating when the cache item will expire, or <c>null</c> if it has no expiration.</param>
public record CacheItem<TValue>(TValue Value, DateTimeOffset CreatedAt, DateTimeOffset? ExpiredAt);
