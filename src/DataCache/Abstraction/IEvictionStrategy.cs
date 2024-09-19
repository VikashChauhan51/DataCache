namespace DataCache.Abstraction;

/// <summary>
/// Defines an eviction strategy interface for tracking and evicting cache items based on a specific eviction policy.
/// Implementations of this interface are responsible for determining which items should be evicted when necessary.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify cache items. Must implement <see cref="IEquatable{TKey}"/> and cannot be null.</typeparam>
public interface IEvictionStrategy<TKey>
    where TKey : notnull, IEquatable<TKey>
{
    /// <summary>
    /// Gets the maximum allowed size of the cache (in bytes or other units), beyond which items will be evicted.
    /// </summary>
    long MaxSize { get; }

    /// <summary>
    /// Gets the current size of the cache (in bytes or other units), which is the total size of all items currently being tracked.
    /// </summary>
    long CurrentSize { get; }

    /// <summary>
    /// Adds a new item to the eviction tracking system.
    /// This method is called when a new item is added to the cache.
    /// </summary>
    /// <param name="key">The key of the item being added.</param>
    void OnItemAdded(TKey key);

    /// <summary>
    /// Marks an item as accessed.
    /// This method should be called whenever a cache item is retrieved, to update its usage within the eviction policy.
    /// </summary>
    /// <param name="key">The key of the item that was accessed.</param>
    void OnItemAccessed(TKey key);

    /// <summary>
    /// Removes an item from the eviction tracking system.
    /// This method is called when an item is removed from the cache.
    /// </summary>
    /// <param name="key">The key of the item to be removed.</param>
    /// <param name="size">The size of the item being removed. Used for tracking the total cache size.</param>
    void OnItemRemoved(TKey key, long size);

    /// <summary>
    /// Evicts an item from the cache based on the eviction policy.
    /// This method is called when the cache exceeds its capacity and an item must be removed to make space.
    /// </summary>
    /// <returns>
    /// The key of the evicted item.
    /// </returns>
    TKey GetEvictionKey();
}
