
using DataCache.Abstraction;
using DataCache.Cache;
using DataCache.Configurations;

namespace DataCache.Test.Cache;

[Trait("Category", "Unit")]

public class CacheTests
{
    private readonly IDataProviderAsync<string, string> _provider;
    private readonly CacheOptions _cacheOptions;
    private readonly Cache<string, string> _cache;

    public CacheTests()
    {
        _provider = A.Fake<IDataProviderAsync<string, string>>();
        _cacheOptions = new CacheOptions(1024, TimeSpan.FromMinutes(5));
        _cache = new Cache<string, string>(_provider, _cacheOptions);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCachedItem()
    {
        var key = "key1";
        var value = "value1";

        A.CallTo(() => _provider.GetAsync(key)).Returns(value);

        var result = await _cache.GetAsync(key);

        result.Should().Be(value);
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldReturnExistingItem()
    {
        var key = "key1";
        var value = "value1";

        A.CallTo(() => _provider.GetAsync(key)).Returns(value);

        var result = await _cache.GetOrSetAsync(key, async () => "newValue", TimeSpan.FromMinutes(10));

        result.Should().Be(value);
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldStoreNewItem()
    {
        var key = "key1";
        var value = "newValue";

 
        A.CallTo(() => _provider.AddAsync(key, value, A<TimeSpan?>._)).DoesNothing();

        var result = await _cache.GetOrSetAsync(key, async () => value, TimeSpan.FromMinutes(10));

        result.Should().Be(value);
        A.CallTo(() => _provider.AddAsync(key, value, A<TimeSpan?>._)).MustHaveHappened();
    }

    [Fact]
    public async Task SetAsync_ShouldCallProviderAddAsync()
    {
        var key = "key1";
        var value = "value1";
        var ttl = TimeSpan.FromMinutes(5);

        A.CallTo(() => _provider.AddAsync(key, value, ttl)).DoesNothing();

        await _cache.SetAsync(key, value, ttl);

        A.CallTo(() => _provider.AddAsync(key, value, ttl)).MustHaveHappened();
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallProviderRemoveAsync()
    {
        var key = "key1";

        A.CallTo(() => _provider.RemoveAsync(key)).DoesNothing();

        await _cache.RemoveAsync(key);

        A.CallTo(() => _provider.RemoveAsync(key)).MustHaveHappened();
    }
}
