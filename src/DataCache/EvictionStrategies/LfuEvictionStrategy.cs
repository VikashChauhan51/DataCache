namespace DataCache.EvictionStrategies;


/// <summary>
/// This strategy tracks the frequency of access to determine which key is the least frequently used.
/// </summary>
public class LfuEvictionStrategy : IEvictionStrategyAsync<string>
{
    private readonly Dictionary<string, int> _accessCounts = new();
    private readonly SortedDictionary<int, HashSet<string>> _frequencyMap = new();
    private readonly object _lock = new();

    /// <inheritdoc />
    public Task AccessItemAsync(string key)
    {
        lock (_lock)
        {
            if (_accessCounts.TryGetValue(key, out var oldFrequency))
            {
                // Update frequency map by removing from old frequency and adding to new frequency
                _frequencyMap[oldFrequency].Remove(key);
                if (_frequencyMap[oldFrequency].Count == 0)
                {
                    _frequencyMap.Remove(oldFrequency);
                }

                var newFrequency = oldFrequency + 1;
                _accessCounts[key] = newFrequency;

                if (!_frequencyMap.TryGetValue(newFrequency, out var keysAtNewFrequency))
                {
                    keysAtNewFrequency = new HashSet<string>();
                    _frequencyMap[newFrequency] = keysAtNewFrequency;
                }
                keysAtNewFrequency.Add(key);
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task AddItemAsync(string key)
    {
        lock (_lock)
        {
            // Initialize frequency to 1 for new items
            _accessCounts[key] = 1;
            if (!_frequencyMap.TryGetValue(1, out var keysAtFrequencyOne))
            {
                keysAtFrequencyOne = new HashSet<string>();
                _frequencyMap[1] = keysAtFrequencyOne;
            }
            keysAtFrequencyOne.Add(key);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveItemAsync(string key)
    {
        lock (_lock)
        {
            if (_accessCounts.TryGetValue(key, out var frequency))
            {
                // Remove the key from the frequency map and access counts
                _frequencyMap[frequency].Remove(key);
                if (_frequencyMap[frequency].Count == 0)
                {
                    _frequencyMap.Remove(frequency);
                }
                _accessCounts.Remove(key);
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> EvictItemAsync()
    {
        lock (_lock)
        {
            if (_frequencyMap.Count > 0)
            {
                // Get the set of items with the least frequency (first entry in SortedDictionary)
                var leastFrequent = _frequencyMap.First();
                var key = leastFrequent.Value.First();

                // Remove the evicted key from the frequency map and access counts
                leastFrequent.Value.Remove(key);
                if (leastFrequent.Value.Count == 0)
                {
                    _frequencyMap.Remove(leastFrequent.Key);
                }
                _accessCounts.Remove(key);

                return Task.FromResult(key);
            }
        }
        return Task.FromResult<string>(default!);
    }
}
