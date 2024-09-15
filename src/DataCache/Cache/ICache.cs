namespace DataCache.Cache;

/// <summary>
/// Defines an asynchronous cache interface for managing key-value pairs 
/// with support for adding, retrieving, and deleting cached items. 
/// This interface supports asynchronous operations for better scalability.
/// </summary>
public interface ICacheAsync
{
    /// <summary>
    /// Asynchronously deletes the cache entry associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the cache item to be deleted.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteAsync(string key);

    /// <summary>
    /// Asynchronously retrieves the cached value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the cache item to retrieve.</param>
    /// <returns>A task that represents the asynchronous get operation. 
    /// The task result contains the cached item if found, otherwise null. See <see cref="CacheItem"/> for details.</returns>
    Task<CacheItem?> GetAsync(string key);

    /// <summary>
    /// Asynchronously adds or updates a cache entry with the specified key and value.
    /// </summary>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="item">The cache item to add or update. See <see cref="CacheItem"/> for details.</param>
    /// <returns>A task that represents the asynchronous put operation.</returns>
    Task PutAsync(string key, CacheItem item);

    /// <summary>
    /// Gets the total number of items currently stored in the cache.
    /// </summary>
    long Count { get; }
}

