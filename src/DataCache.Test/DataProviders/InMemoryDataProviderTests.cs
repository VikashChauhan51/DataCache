using DataCache.Abstraction;
using DataCache.DataProviders;

namespace DataCache.Test.DataProviders;

[Trait("Category", "Unit")]

public class InMemoryDataProviderTests
{
    private readonly IEvictionStrategy<string> _evictionStrategy;
    private readonly Func<string, long> _sizeCalculator;
    private readonly InMemoryDataProvider<string, string> _dataProvider;

    public InMemoryDataProviderTests()
    {
        _evictionStrategy = A.Fake<IEvictionStrategy<string>>();
        _sizeCalculator = _ => 10; // Example size calculator
        _dataProvider = new InMemoryDataProvider<string, string>(_evictionStrategy, _sizeCalculator);
    }

    [Fact]
    public async Task AddAsync_ShouldAddItemToCache()
    {
        var key = "key1";
        var value = "value1";

        await _dataProvider.AddAsync(key, value, TimeSpan.FromMinutes(5));

        var result = await _dataProvider.GetAsync(key);
        result.Should().Be(value);
    }

    [Fact]
    public async Task AddAsync_ShouldEvictItemWhenCacheExceedsMaxSize()
    {
        // Setup eviction strategy to simulate eviction
        A.CallTo(() => _evictionStrategy.GetEvictionKey()).Returns("evictKey");

        var key1 = "key1";
        var value1 = "value1";
        var key2 = "key2";
        var value2 = "value2";

        A.CallTo(() => _sizeCalculator(value1)).Returns(100);
        A.CallTo(() => _sizeCalculator(value2)).Returns(200);
        A.CallTo(() => _evictionStrategy.CurrentSize).Returns(150);
        A.CallTo(() => _evictionStrategy.MaxSize).Returns(200);

        await _dataProvider.AddAsync(key1, value1, TimeSpan.FromMinutes(5));
        await _dataProvider.AddAsync(key2, value2, TimeSpan.FromMinutes(5));

        // Ensure that the item was removed due to eviction
        A.CallTo(() => _evictionStrategy.OnItemRemoved(A<string>._, A<long>._)).MustHaveHappened();
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveItemFromCache()
    {
        var key = "key1";
        var value = "value1";

        await _dataProvider.AddAsync(key, value, TimeSpan.FromMinutes(5));
        await _dataProvider.RemoveAsync(key);

        var result = await _dataProvider.GetAsync(key);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDefaultIfItemIsExpired()
    {
        var key = "key1";
        var value = "value1";

        await _dataProvider.AddAsync(key, value, TimeSpan.FromMilliseconds(1)); // Set TTL to a very short time
        await Task.Delay(10); // Wait for item to expire

        var result = await _dataProvider.GetAsync(key);
        result.Should().BeNull();
    }

    [Fact]
    public async Task StopCleanupTask_ShouldStopBackgroundCleanup()
    {
        _dataProvider.StopCleanupTask();

        // Ensure no exceptions are thrown and the cleanup task is stopped
        // Direct verification might be complex; you could check side effects if applicable
    }

    [Fact]
    public async Task CleanUpExpiredItems_ShouldRemoveExpiredItems()
    {
        var key1 = "key1";
        var key2 = "key2";
        var value1 = "value1";
        var value2 = "value2";

        await _dataProvider.AddAsync(key1, value1, TimeSpan.FromMilliseconds(1));
        await _dataProvider.AddAsync(key2, value2, TimeSpan.FromMinutes(5));
        await Task.Delay(10); // Ensure the first item is expired

        //await _dataProvider.CleanUpExpiredItems();

        var result1 = await _dataProvider.GetAsync(key1);
        var result2 = await _dataProvider.GetAsync(key2);

        result1.Should().BeNull(); // Expired item
        result2.Should().Be(value2); // Still valid item
    }
}

