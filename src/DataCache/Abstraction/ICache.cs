namespace DataCache.Abstraction;

/// <summary>
/// Defines an asynchronous cache interface with basic operations for getting, setting, and removing cache items.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify cache entries. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
/// <typeparam name="TValue">The type of the value to be cached.</typeparam>
public interface ICacheAsync<in TKey, TValue>
    where TKey : notnull, IEquatable<TKey>
{
    /// <summary>
    /// Retrieves the cached value associated with the specified key asynchronously.
    /// </summary>
    /// <param name="key">The key of the cached item to retrieve.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the cached value.</returns>
    Task<TValue> GetAsync(TKey key);

    /// <summary>
    /// Retrieves the cached value if it exists; otherwise, sets and returns the value using the provided factory function asynchronously.
    /// </summary>
    /// <param name="key">The key of the cached item.</param>
    /// <param name="valueFactory">A factory function that asynchronously produces the value to cache if it doesn't already exist.</param>
    /// <param name="ttl">An optional time-to-live (TTL) value that defines the cache expiration. If <c>null</c>, the item does not expire.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the value retrieved or newly set.</returns>
    Task<TValue> GetOrSetAsync(TKey key, Func<Task<TValue>> valueFactory, TimeSpan? ttl);

    /// <summary>
    /// Asynchronously sets the value in the cache for the specified key, with an optional expiration time (TTL).
    /// </summary>
    /// <param name="key">The key to associate with the cached value.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="ttl">An optional time-to-live (TTL) value that defines when the cache item expires. If <c>null</c>, the item does not expire.</param>
    /// <returns>A task representing the asynchronous set operation.</returns>
    Task SetAsync(TKey key, TValue value, TimeSpan? ttl);

    /// <summary>
    /// Asynchronously removes the cached value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the cache item to remove.</param>
    /// <returns>A task representing the asynchronous remove operation.</returns>
    Task RemoveAsync(TKey key);
}
