namespace DataCache.EvictionStrategies;


/// <summary>
/// Default no-op eviction strategy. This strategy does not track or evict any items from the cache.
/// It is useful when no eviction policy is needed (e.g., infinite capacity cache).
/// </summary>
public class DefaultEvictionStrategy : IEvictionStrategyAsync<string>
{
    /// <inheritdoc />
    /// <summary>
    /// No-op implementation of the access item operation. No tracking is performed for the accessed item.
    /// </summary>
    /// <param name="key">The key of the accessed item.</param>
    /// <returns>A completed task.</returns>
    public Task AccessItemAsync(string key)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <summary>
    /// No-op implementation of the add item operation. No tracking is performed for the added item.
    /// </summary>
    /// <param name="key">The key of the item being added.</param>
    /// <returns>A completed task.</returns>
    public Task AddItemAsync(string key)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <summary>
    /// No-op implementation of the eviction operation. Always returns the default key (null) as no items are tracked.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the result being the default value of the key type (null).</returns>
    public Task<string> EvictItemAsync()
    {
        return Task.FromResult<string>(default!);
    }

    /// <inheritdoc />
    /// <summary>
    /// No-op implementation of the remove item operation. No tracking is performed for the removed item.
    /// </summary>
    /// <param name="key">The key of the item being removed.</param>
    /// <returns>A completed task.</returns>
    public Task RemoveItemAsync(string key)
    {
        return Task.CompletedTask;
    }
}

