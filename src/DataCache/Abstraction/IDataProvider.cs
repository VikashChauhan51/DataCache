

namespace DataCache.Abstraction;

public interface IDataProviderAsync<in TKey, TValue> where TKey : notnull, IEquatable<TKey>
{
    Task AddAsync(TKey key, TValue value, TimeSpan? ttl);
    Task<TValue> GetAsync(TKey key);
    Task RemoveAsync(TKey key);

}
