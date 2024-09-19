namespace DataCache.Abstraction;


/// <summary>
/// Defines an eviction strategy interface for tracking and evicting cache items based on a specific eviction policy.
/// Implementations of this interface are responsible for determining which items should be evicted when necessary.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify cache items.</typeparam>
public interface IEvictionStrategy<TKey> where TKey : notnull, IEquatable<TKey>
{
    /// <summary>
    /// Adds a new item to the eviction tracking system.
    /// This method is called when a new item is added to the cache.
    /// </summary>
    /// <param name="key">The key of the item being added.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    void OnItemAdded(TKey key);

    /// <summary>
    /// Marks an item as accessed. This method should be called whenever a cache item is retrieved to update its usage.
    /// </summary>
    /// <param name="key">The key of the item that was accessed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    void OnItemAccessed(TKey key);

    /// <summary>
    /// Removes an item from the eviction tracking system.
    /// This method is called when an item is removed from the cache.
    /// </summary>
    /// <param name="key">The key of the item to be removed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    void OnItemRemoved(TKey key, long size);

    /// <summary>
    /// Evicts an item from the cache based on the eviction policy.
    /// This method is called when the cache exceeds its capacity and an item must be removed to make space.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous eviction operation. The task result contains the key of the evicted item.
    /// </returns>
    TKey GetEvictionKey();

    public long MaxSize { get; }

    public long CurrentSize { get;}

}
