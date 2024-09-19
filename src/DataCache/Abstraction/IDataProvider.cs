namespace DataCache.Abstraction;

/// <summary>
/// Defines an asynchronous data provider interface with basic operations for adding, retrieving, and removing data.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify data entries. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
/// <typeparam name="TValue">The type of the value to be managed by the data provider.</typeparam>
public interface IDataProviderAsync<in TKey, TValue>
    where TKey : notnull, IEquatable<TKey>
{
    /// <summary>
    /// Asynchronously adds a value to the data store with an optional expiration time (TTL).
    /// </summary>
    /// <param name="key">The key to associate with the value.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="ttl">An optional time-to-live (TTL) value that defines when the data entry expires. If <c>null</c>, the entry does not expire.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddAsync(TKey key, TValue value, TimeSpan? ttl);

    /// <summary>
    /// Retrieves the value associated with the specified key asynchronously.
    /// </summary>
    /// <param name="key">The key of the data entry to retrieve.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the value associated with the specified key.</returns>
    Task<TValue> GetAsync(TKey key);

    /// <summary>
    /// Asynchronously removes the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the data entry to remove.</param>
    /// <returns>A task representing the asynchronous remove operation.</returns>
    Task RemoveAsync(TKey key);
}
