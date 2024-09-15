using System.Collections.Concurrent;

namespace DataCache.EvictionStrategies;


/// <summary>
/// The LRU (Least Recently Used) strategy evicts the least recently accessed items.
/// </summary>
public class LruEvictionStrategy : IEvictionStrategyAsync<string>
{
    private readonly LinkedList<string> _accessOrder = new();
    private readonly ConcurrentDictionary<string, LinkedListNode<string>> _keyNodes = new();
    private readonly object _lock = new(); 

    /// <inheritdoc />
    public Task AccessItemAsync(string key)
    {
        if (_keyNodes.TryGetValue(key, out var node))
        {
            lock (_lock)
            {
                _accessOrder.Remove(node);
                _accessOrder.AddLast(node);
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task AddItemAsync(string key)
    {
        var node = new LinkedListNode<string>(key);
        if (_keyNodes.TryAdd(key, node))
        {
            lock (_lock)
            {
                _accessOrder.AddLast(node);
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveItemAsync(string key)
    {
        if (_keyNodes.TryRemove(key, out var node))
        {
            lock (_lock)
            {
                _accessOrder.Remove(node);
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> EvictItemAsync()
    {
        lock (_lock)
        {
            var oldest = _accessOrder.First; 
            if (oldest != null)
            {
                _accessOrder.RemoveFirst(); 
                _keyNodes.TryRemove(oldest.Value, out _); 
                return Task.FromResult(oldest.Value);
            }
        }
        return Task.FromResult<string>(default!);
    }
}

