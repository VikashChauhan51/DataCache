namespace DataCache.Cache;

public class FastInMemoryCache<T> : IDisposable
{
    private readonly Cache<T> _cache = new();
    private readonly Multiplexer _multiplexer = new();

    public async Task AddAsync(string key, T value, TimeSpan ttl)
    {
        await _multiplexer.EnqueueAsync(() => _cache.Set(key, value, ttl));
    }

    public async Task<T?> GetAsync(string key)
    {
        return await _multiplexer.EnqueueAsync(() => _cache.Get(key));
    }

    public async Task RemoveAsync(string key)
    {
        await _multiplexer.EnqueueAsync(() => _cache.Remove(key));
    }

    public void Dispose()
    {
        _multiplexer.Dispose();
        _cache.Dispose();
    }
}
