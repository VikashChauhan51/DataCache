using DataCache.Configurations;
using StackExchange.Redis;

namespace DataCache.Cache;

public class RedisCache : CacheBase, ICache
{
    private readonly IDatabase _redisDatabase;
    private readonly RedisCacheOptions _options;
    private readonly MemoryCache _inMemoryCache;

    public long Count => _inMemoryCache.Count;

    public RedisCache(RedisCacheOptions options, IConnectionMultiplexer connectionMultiplexer) : base(options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _redisDatabase = connectionMultiplexer.GetDatabase(_options.DatabaseIndex);
        _inMemoryCache = new MemoryCache(options);
    }

    public string Get(string key)
    {
        if (_options.Optimized && _inMemoryCache.TryGet(key, out var cacheItem))
        {
            return cacheItem;
        }

        var value = _redisDatabase.StringGet(key);
        if (value.HasValue && _options.Optimized)
        {
            // Cache in memory if optimized
            _inMemoryCache.Put(key, value!);
        }
        return value!;
    }

    public void Put(string key, string value)
    {


        if (_options.Optimized)
        {
            // Add to in-memory cache
            _inMemoryCache.Put(key, value);
        }

        _redisDatabase.StringSet(key, value);
    }

    public bool TryGet(string key, out string value)
    {
        if (_options.Optimized && _inMemoryCache.TryGet(key, out var cacheItem))
        {
            value = cacheItem;
            return true;
        }

        var redisValue = _redisDatabase.StringGet(key);
        if (redisValue.HasValue)
        {
            value = redisValue!;
            if (_options.Optimized)
            {
                // Cache in memory if optimized
                _inMemoryCache.Put(key, value!);
            }
            return true;
        }

        value = default!;
        return false;
    }

    public void Delete(string key)
    {
        _inMemoryCache.TryDelete(key);
        _redisDatabase.StringGetDelete(key);
    }

    public bool TryDelete(string key)
    {
        try
        {
            _inMemoryCache.TryDelete(key);
            _redisDatabase.StringGetDelete(key);
        }
        catch
        {

            return false;
        }
        return true;
    }
}