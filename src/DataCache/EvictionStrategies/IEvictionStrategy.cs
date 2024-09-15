namespace DataCache.EvictionStrategies;

/// <summary>
/// Defines an asynchronous eviction strategy interface for tracking and evicting cache items based on a specific eviction policy.
/// Implementations of this interface are responsible for determining which items should be evicted when necessary.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify cache items.</typeparam>
public interface IEvictionStrategyAsync<TKey>
{
    /// <summary>
    /// Asynchronously marks an item as accessed. This method should be called whenever a cache item is retrieved to update its usage.
    /// </summary>
    /// <param name="key">The key of the item that was accessed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AccessItemAsync(TKey key);

    /// <summary>
    /// Asynchronously adds a new item to the eviction tracking system.
    /// This method is called when a new item is added to the cache.
    /// </summary>
    /// <param name="key">The key of the item being added.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddItemAsync(TKey key);

    /// <summary>
    /// Asynchronously removes an item from the eviction tracking system.
    /// This method is called when an item is removed from the cache.
    /// </summary>
    /// <param name="key">The key of the item to be removed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveItemAsync(TKey key);

    /// <summary>
    /// Asynchronously evicts an item from the cache based on the eviction policy.
    /// This method is called when the cache exceeds its capacity and an item must be removed to make space.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous eviction operation. The task result contains the key of the evicted item.
    /// </returns>
    Task<TKey> EvictItemAsync();
}
