using DataCache.Abstraction;

namespace DataCache.EvictionStrategies;


/// <summary>
/// This strategy tracks the order of keys to determine which one is the most recently used.
/// </summary>
public class MruEvictionStrategy<TKey> : IEvictionStrategy<TKey> where TKey : notnull, IEquatable<TKey>
{
    private readonly LinkedList<TKey> _accessOrder = new();
    private readonly Dictionary<TKey, LinkedListNode<TKey>> _cacheMap = new();
    private readonly object _lock = new();
 

    /// <inheritdoc />
    public void OnItemAdded(TKey key)
    {
        lock (_lock)
        {
            if (!_cacheMap.ContainsKey(key))
            {
                var node = new LinkedListNode<TKey>(key);
                _accessOrder.AddFirst(node);
                _cacheMap[key] = node;
            }
        }
    }


    /// <inheritdoc />
    public void OnItemAccessed(TKey key)
    {
        lock (_lock)
        {
            if (_cacheMap.TryGetValue(key, out var node))
            {
                _accessOrder.Remove(node);
                _accessOrder.AddFirst(node);
            }
        }
    }


    /// <inheritdoc />
    public void OnItemRemoved(TKey key)
    {
        lock (_lock)
        {
            if (_cacheMap.TryGetValue(key, out var node))
            {
                _accessOrder.Remove(node);
                _cacheMap.Remove(key);
            }
        }
    }

    /// <inheritdoc />
    public TKey GetEvictionKey()
    {
        lock (_lock)
        {
            var mostRecent = _accessOrder.First;
            if (mostRecent != null)
            {
                _accessOrder.RemoveFirst();
                _cacheMap.Remove(mostRecent.Value);
                return mostRecent.Value;
            }
        }

        return default!;
    }
}
