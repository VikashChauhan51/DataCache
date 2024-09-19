

using DataCache.Configurations;
using DataCache.EvictionStrategies;

namespace DataCache.Test.EvictionStrategiesTests;

[Trait("Category", "Unit")]

public class LruEvictionStrategyTests
{
    private readonly CacheOptions _cacheOptions;
    private readonly LruEvictionStrategy<string> _lruStrategy;

    public LruEvictionStrategyTests()
    {
        _cacheOptions = new CacheOptions(maxMemorySize: 1000, ttlInterval: TimeSpan.FromMinutes(10));
        _lruStrategy = new LruEvictionStrategy<string>(_cacheOptions);
    }

    [Fact]
    public void OnItemAdded_ShouldIncreaseUsageTracking()
    {
        var key = "item1";
        _lruStrategy.OnItemAdded(key);

        _lruStrategy.CurrentSize.Should().Be(0); // Assuming size is not tracked in this method
        var evictionKey = _lruStrategy.GetEvictionKey();
        evictionKey.Should().Be(key);
    }

    [Fact]
    public void OnItemAccessed_ShouldUpdateRecentUsage()
    {
        var key = "item1";
        _lruStrategy.OnItemAdded(key);
        _lruStrategy.OnItemAccessed(key);

        // Verify that the item is marked as recently used
        var evictionKey = _lruStrategy.GetEvictionKey();
        evictionKey.Should().Be(key); // Should still be the same item if accessed
    }

    [Fact]
    public void OnItemRemoved_ShouldDecreaseUsageTracking()
    {
        var key = "item1";
        _lruStrategy.OnItemAdded(key);
        _lruStrategy.OnItemRemoved(key, 10);

        var evictionKey = _lruStrategy.GetEvictionKey();
        evictionKey.Should().BeNull(); // No items should be present
    }

    [Fact]
    public void GetEvictionKey_ShouldReturnLeastRecentlyUsedKey()
    {
        var key1 = "item1";
        var key2 = "item2";
        var key3 = "item3";

        _lruStrategy.OnItemAdded(key1);
        _lruStrategy.OnItemAdded(key2);
        _lruStrategy.OnItemAdded(key3);

        _lruStrategy.OnItemAccessed(key1); // Make key1 recently used
        _lruStrategy.OnItemAccessed(key2); // Make key2 recently used

        var evictionKey = _lruStrategy.GetEvictionKey();
        evictionKey.Should().Be(key3); // Should be the least recently used key
    }
}
