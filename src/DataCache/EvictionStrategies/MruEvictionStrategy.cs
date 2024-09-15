namespace DataCache.EvictionStrategies;


/// <summary>
/// This strategy tracks the order of keys to determine which one is the most recently used.
/// </summary>
public class MruEvictionStrategy : IEvictionStrategyAsync<string>
{
    private readonly LinkedList<string> _accessOrder = new();
    private readonly Dictionary<string, LinkedListNode<string>> _cacheMap = new();
    private readonly object _lock = new();

    /// <inheritdoc />
    public Task AccessItemAsync(string key)
    {
        lock (_lock)
        {
            if (_cacheMap.TryGetValue(key, out var node))
            {
                _accessOrder.Remove(node);
                _accessOrder.AddFirst(node);
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task AddItemAsync(string key)
    {
        lock (_lock)
        {
            if (!_cacheMap.ContainsKey(key))
            {
                var node = new LinkedListNode<string>(key);
                _accessOrder.AddFirst(node);
                _cacheMap[key] = node;
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveItemAsync(string key)
    {
        lock (_lock)
        {
            if (_cacheMap.TryGetValue(key, out var node))
            {
                _accessOrder.Remove(node);
                _cacheMap.Remove(key);
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> EvictItemAsync()
    {
        lock (_lock)
        {
            var mostRecent = _accessOrder.First;
            if (mostRecent != null)
            {
                _accessOrder.RemoveFirst();
                _cacheMap.Remove(mostRecent.Value);
                return Task.FromResult(mostRecent.Value);
            }
        }
        return Task.FromResult<string>(default!);
    }
}
