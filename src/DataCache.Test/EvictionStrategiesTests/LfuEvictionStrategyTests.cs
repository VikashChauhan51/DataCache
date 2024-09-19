using DataCache.Configurations;
using DataCache.EvictionStrategies;

namespace DataCache.Test.EvictionStrategiesTests;

[Trait("Category", "Unit")]
public class LfuEvictionStrategyTests
{
    private readonly CacheOptions _cacheOptions;
    private readonly LfuEvictionStrategy<string> _lfuStrategy;

    public LfuEvictionStrategyTests()
    {
        _cacheOptions = new CacheOptions(maxMemorySize: 1000, ttlInterval: TimeSpan.FromMinutes(10));
        _lfuStrategy = new LfuEvictionStrategy<string>(_cacheOptions);
    }

    [Fact]
    public void OnItemAdded_ShouldIncreaseFrequencyMapSize()
    {
        var key = "item1";
        _lfuStrategy.OnItemAdded(key);

        _lfuStrategy.CurrentSize.Should().Be(0); // Assuming size is not tracked in this method
        var evictionKey = _lfuStrategy.GetEvictionKey();
        evictionKey.Should().Be(key);
    }

    [Fact]
    public void OnItemAccessed_ShouldUpdateFrequency()
    {
        var key = "item1";
        _lfuStrategy.OnItemAdded(key);
        _lfuStrategy.OnItemAccessed(key);

        _lfuStrategy.GetEvictionKey().Should().Be(key); // Should be the same item
    }

    [Fact]
    public void OnItemRemoved_ShouldDecreaseFrequencyMapSize()
    {
        var key = "item1";
        _lfuStrategy.OnItemAdded(key);
        _lfuStrategy.OnItemRemoved(key, 10);

        var evictionKey = _lfuStrategy.GetEvictionKey();
        evictionKey.Should().BeNull(); // No items should be present
    }

    [Fact]
    public void GetEvictionKey_ShouldReturnLeastFrequentlyUsedKey()
    {
        var key1 = "item1";
        var key2 = "item2";
        var key3 = "item3";

        _lfuStrategy.OnItemAdded(key1);
        _lfuStrategy.OnItemAdded(key2);
        _lfuStrategy.OnItemAdded(key3);

        _lfuStrategy.OnItemAccessed(key1);
        _lfuStrategy.OnItemAccessed(key1);
        _lfuStrategy.OnItemAccessed(key2);

        var evictionKey = _lfuStrategy.GetEvictionKey();
        evictionKey.Should().Be(key3); // Should be the least frequently used key
    }
}

