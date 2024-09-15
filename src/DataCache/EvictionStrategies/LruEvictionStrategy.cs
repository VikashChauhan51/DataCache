namespace DataCache.EvictionStrategies;


/// <summary>
/// The LRU strategy evicts the least recently accessed items.
/// </summary>
public class LruEvictionStrategy : IEvictionStrategy<string>
{
    private readonly LinkedList<string> _accessOrder = new();
    private readonly Dictionary<string, LinkedListNode<string>> _keyNodes = new();
    private readonly object _lock = new();
    public void AccessItem(string key)
    {
        lock (_lock)
        {
            if (_keyNodes.ContainsKey(key))
            {
                var node = _keyNodes[key];
                _accessOrder.Remove(node);
                _accessOrder.AddLast(node);
            }
        }
    }

    public void AddItem(string key)
    {
        lock (_lock)
        {
            var node = new LinkedListNode<string>(key);
            _accessOrder.AddLast(node);
            _keyNodes[key] = node;
        }
    }

    public void RemoveItem(string key)
    {
        lock (_lock)
        {
            if (_keyNodes.TryGetValue(key, out var node))
            {
                _accessOrder.Remove(node);
                _keyNodes.Remove(key);
            }
        }
    }

    public string EvictItem()
    {
        lock (_lock)
        {
            var oldest = _accessOrder.First;
            if (oldest != null)
            {
                _accessOrder.RemoveFirst();
                _keyNodes.Remove(oldest.Value);
                return oldest.Value;
            }

            throw new InvalidOperationException("No items to evict");
        }
    }
}

