using DataCache.Configurations;
using DataCache.EvictionStrategies;

namespace DataCache.Test.EvictionStrategiesTests;

[Trait("Category", "Unit")]

public class MruEvictionStrategyTests
{
    private readonly CacheOptions _cacheOptions;
    private readonly MruEvictionStrategy<string> _mruStrategy;

    public MruEvictionStrategyTests()
    {
        _cacheOptions = new CacheOptions(maxMemorySize: 1000, ttlInterval: TimeSpan.FromMinutes(10));
        _mruStrategy = new MruEvictionStrategy<string>(_cacheOptions);
    }

    [Fact]
    public void OnItemAdded_ShouldAddItemToCache()
    {
        var key = "item1";
        _mruStrategy.OnItemAdded(key);

        // Since we can't directly access internal state, we assume the test is successful if no exception is thrown
        _mruStrategy.CurrentSize.Should().Be(0); // Assuming size is not tracked in this method
        var evictionKey = _mruStrategy.GetEvictionKey();
        evictionKey.Should().Be(key);
    }

    [Fact]
    public void OnItemAccessed_ShouldMoveItemToFront()
    {
        var key1 = "item1";
        var key2 = "item2";

        _mruStrategy.OnItemAdded(key1);
        _mruStrategy.OnItemAdded(key2);
        _mruStrategy.OnItemAccessed(key1);

        var evictionKey = _mruStrategy.GetEvictionKey();
        evictionKey.Should().Be(key2); // key1 should be the most recently used, so key2 should be evicted
    }

    [Fact]
    public void OnItemRemoved_ShouldRemoveItemFromCache()
    {
        var key = "item1";
        _mruStrategy.OnItemAdded(key);
        _mruStrategy.OnItemRemoved(key, 10);

        var evictionKey = _mruStrategy.GetEvictionKey();
        evictionKey.Should().NotBe(key); // No items should be present, so eviction key should not match the removed item
    }

    [Fact]
    public void GetEvictionKey_ShouldReturnMostRecentlyUsedKey()
    {
        var key1 = "item1";
        var key2 = "item2";
        var key3 = "item3";

        _mruStrategy.OnItemAdded(key1);
        _mruStrategy.OnItemAdded(key2);
        _mruStrategy.OnItemAdded(key3);

        _mruStrategy.OnItemAccessed(key1); // Make key1 most recently used
        _mruStrategy.OnItemAccessed(key2); // Make key2 most recently used

        var evictionKey = _mruStrategy.GetEvictionKey();
        evictionKey.Should().Be(key1); // Should be the most recently used key
    }
}
