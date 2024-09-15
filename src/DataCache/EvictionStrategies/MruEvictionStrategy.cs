namespace DataCache.EvictionStrategies;


/// <summary>
/// This strategy tracks the order of keys to determine which one is the most recently used.
/// </summary>
public class MruEvictionStrategy : IEvictionStrategy<string>
{
    private readonly LinkedList<string> _accessOrder = new();
    private readonly Dictionary<string, LinkedListNode<string>> _cacheMap = new();
    private readonly object _lock = new();
    public void AccessItem(string key)
    {
        lock (_lock)
        {
            if (_cacheMap.ContainsKey(key))
            {
                var node = _cacheMap[key];
                _accessOrder.Remove(node);
                _accessOrder.AddFirst(node);
            }
        }
    }

    public void AddItem(string key)
    {
        lock (_lock)
        {
            var node = new LinkedListNode<string>(key);
            _accessOrder.AddFirst(node);
            _cacheMap[key] = node;
        }
    }

    public void RemoveItem(string key)
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

    public string EvictItem()
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

            throw new InvalidOperationException("No items to evict");
        }
    }
}
