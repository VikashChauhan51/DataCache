namespace DataCache.Abstraction;

public interface ICacheAsync<in TKey, TValue> where TKey : notnull, IEquatable<TKey>
{
    Task<TValue> GetAsync(TKey key);
    Task<TValue> GetOrSetAsync(TKey key, Func<Task<TValue>> valueFactory, TimeSpan? ttl);
    Task SetAsync(TKey key, TValue value, TimeSpan? ttl);
    Task RemoveAsync(TKey key);
}
