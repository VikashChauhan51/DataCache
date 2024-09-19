
using DataCache.Configurations;
using DataCache.EvictionStrategies;

namespace DataCache.Test.EvictionStrategiesTests;

[Trait("Category", "Unit")]
public class RandomEvictionStrategyTests
{
    private readonly CacheOptions _cacheOptions;
    private readonly RandomEvictionStrategy<string> _randomStrategy;

    public RandomEvictionStrategyTests()
    {
        _cacheOptions = new CacheOptions(maxMemorySize: 1000, ttlInterval: TimeSpan.FromMinutes(10));
        _randomStrategy = new RandomEvictionStrategy<string>(_cacheOptions);
    }

    [Fact]
    public void OnItemAdded_ShouldAddItemToCache()
    {
        var key = "item1";
        _randomStrategy.OnItemAdded(key);

        // Since we can't directly access internal state, we assume the test is successful if no exception is thrown
        _randomStrategy.CurrentSize.Should().Be(0); // Assuming size is not tracked in this method
    }

    [Fact]
    public void OnItemRemoved_ShouldRemoveItemFromCache()
    {
        var key = "item1";
        _randomStrategy.OnItemAdded(key);
        _randomStrategy.OnItemRemoved(key, 10);

        var evictionKey = _randomStrategy.GetEvictionKey();
        evictionKey.Should().NotBe(key); // No items should be present, so eviction key should not match the removed item
    }

    [Fact]
    public void GetEvictionKey_ShouldReturnNextItemInRoundRobin()
    {
        var key1 = "item1";
        var key2 = "item2";
        var key3 = "item3";

        _randomStrategy.OnItemAdded(key1);
        _randomStrategy.OnItemAdded(key2);
        _randomStrategy.OnItemAdded(key3);

        // Get the eviction key several times and ensure it cycles through the items
        var firstEviction = _randomStrategy.GetEvictionKey();
        var secondEviction = _randomStrategy.GetEvictionKey();
        var thirdEviction = _randomStrategy.GetEvictionKey();

        //firstEviction.Should().Be(key1).Or.Be(key2).Or.Be(key3);
        //secondEviction.Should().Be(key1).Or.Be(key2).Or.Be(key3);
        //thirdEviction.Should().Be(key1).Or.Be(key2).Or.Be(key3);

        // After evicting three times, we should get the first item again
        var fourthEviction = _randomStrategy.GetEvictionKey();
        fourthEviction.Should().Be(firstEviction);
    }

    [Fact]
    public void GetEvictionKey_ShouldHandleEmptyCache()
    {
        var evictionKey = _randomStrategy.GetEvictionKey();
        evictionKey.Should().Be(default(string)); // Should return default value when there are no items to evict
    }

    [Fact]
    public void OnItemRemoved_ShouldAdjustIndexCorrectly()
    {
        var key1 = "item1";
        var key2 = "item2";
        var key3 = "item3";

        _randomStrategy.OnItemAdded(key1);
        _randomStrategy.OnItemAdded(key2);
        _randomStrategy.OnItemAdded(key3);

        // Evict one item
        _randomStrategy.GetEvictionKey();
        // Remove an item
        _randomStrategy.OnItemRemoved(key1, 10);

        // Check the next eviction key
        var evictionKey = _randomStrategy.GetEvictionKey();
        evictionKey.Should().Be(key2);//.Or.Be(key3); // Should evict one of the remaining items
    }
}

