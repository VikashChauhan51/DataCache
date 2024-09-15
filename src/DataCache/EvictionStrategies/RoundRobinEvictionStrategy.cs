

namespace DataCache.EvictionStrategies;

/// <summary>
/// This strategy evicts items in a round-robin fashion, cycling through all keys in order.
/// </summary>
public class RoundRobinEvictionStrategy : IEvictionStrategyAsync<string>
{
    private readonly List<string> _keys = new();
    private int _currentIndex = -1; // Tracks the current index for eviction
    private readonly object _lock = new();

    /// <inheritdoc />
    public Task AccessItemAsync(string key)
    {
        // Round-robin doesn't care about access patterns, so this is a no-op
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task AddItemAsync(string key)
    {
        lock (_lock)
        {
            // Add the new item to the list of keys
            if (!_keys.Contains(key))
            {
                _keys.Add(key);
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveItemAsync(string key)
    {
        lock (_lock)
        {
            if (_keys.Remove(key))
            {
                // Adjust the current index if necessary
                if (_keys.Count > 0 && _currentIndex >= _keys.Count)
                {
                    _currentIndex = _currentIndex % _keys.Count;
                }
                else if (_keys.Count == 0)
                {
                    _currentIndex = -1;
                }
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> EvictItemAsync()
    {
        lock (_lock)
        {
            if (_keys.Count == 0)
            {
                return Task.FromResult<string>(default!); // No item to evict
            }

            // Move to the next item in round-robin fashion
            _currentIndex = (_currentIndex + 1) % _keys.Count;
            var keyToEvict = _keys[_currentIndex];

            // Remove the key from the list
            _keys.RemoveAt(_currentIndex);

            // Adjust the current index, as the list size is reduced
            if (_keys.Count > 0)
            {
                _currentIndex = _currentIndex % _keys.Count;
            }
            else
            {
                _currentIndex = -1; // Reset if no items are left
            }

            return Task.FromResult(keyToEvict);
        }
    }
}

