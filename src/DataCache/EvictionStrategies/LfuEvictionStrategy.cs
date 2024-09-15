namespace DataCache.EvictionStrategies;


/// <summary>
/// This strategy tracks the frequency of access to determine which key is the least frequently used.
/// </summary>
public class LfuEvictionStrategy : IEvictionStrategy<string>
{
    private readonly Dictionary<string, int> _accessCounts = new();
    private readonly SortedDictionary<int, HashSet<string>> _frequencyMap = new();
    private readonly object _lock = new();
    public void AccessItem(string key)
    {
        lock (_lock)
        {
            if (_accessCounts.ContainsKey(key))
            {
                int oldFrequency = _accessCounts[key];
                _accessCounts[key] = oldFrequency + 1;
                _frequencyMap[oldFrequency].Remove(key);
                if (_frequencyMap[oldFrequency].Count == 0)
                {
                    _frequencyMap.Remove(oldFrequency);
                }

                if (!_frequencyMap.ContainsKey(oldFrequency + 1))
                {
                    _frequencyMap[oldFrequency + 1] = new HashSet<string>();
                }
                _frequencyMap[oldFrequency + 1].Add(key);
            }
        }
    }

    public void AddItem(string key)
    {
        lock (_lock)
        {
            _accessCounts[key] = 1;
            if (!_frequencyMap.ContainsKey(1))
            {
                _frequencyMap[1] = new HashSet<string>();
            }
            _frequencyMap[1].Add(key);
        }
    }

    public void RemoveItem(string key)
    {
        lock (_lock)
        {
            if (_accessCounts.TryGetValue(key, out var frequency))
            {
                _frequencyMap[frequency].Remove(key);
                if (_frequencyMap[frequency].Count == 0)
                {
                    _frequencyMap.Remove(frequency);
                }
                _accessCounts.Remove(key);
            }
        }
    }

    public string EvictItem()
    {
        lock (_lock)
        {
            if (_frequencyMap.Count == 0)
            {
                throw new InvalidOperationException("No items to evict");
            }

            var leastFrequent = _frequencyMap.First();
            var key = leastFrequent.Value.First();
            leastFrequent.Value.Remove(key);
            if (leastFrequent.Value.Count == 0)
            {
                _frequencyMap.Remove(leastFrequent.Key);
            }

            _accessCounts.Remove(key);
            return key;
        }
    }
}
