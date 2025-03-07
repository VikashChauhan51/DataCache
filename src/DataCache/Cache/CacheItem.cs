

namespace DataCache.Cache;

internal class CacheItem<T>
{
    public T? Value { get; }
    public DateTime Expiration { get; }
    public bool IsExpired => DateTime.Now >= Expiration;
    public CacheItem(T value, DateTime expiration)
    {
        Value = value;
        Expiration = expiration;
    }
}
