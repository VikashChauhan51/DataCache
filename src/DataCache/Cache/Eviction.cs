namespace DataCache.Cache;

/// <summary>
/// Defines the available eviction strategies that can be used in the cache system.
/// </summary>
public enum Eviction
{
    /// <summary>
    /// No eviction policy is applied. The cache will not automatically remove any items,
    /// and there is no size or item count limit.
    /// </summary>
    None,

    /// <summary>
    /// Least Recently Used (LRU) eviction strategy. Evicts the least recently accessed item
    /// when the cache reaches its limit.
    /// </summary>
    LRU,

    /// <summary>
    /// Least Frequently Used (LFU) eviction strategy. Evicts the item with the lowest access
    /// frequency when the cache reaches its limit.
    /// </summary>
    LFU,

    /// <summary>
    /// Most Recently Used (MRU) eviction strategy. Evicts the most recently accessed item
    /// when the cache reaches its limit.
    /// </summary>
    MRU,

    /// <summary>
    /// Round-Robin eviction strategy. Evicts items in a cyclic order, following the order
    /// in which they were added or accessed, regardless of usage patterns.
    /// </summary>
    RoundRobin
}
