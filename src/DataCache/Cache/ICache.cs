namespace DataCache.Cache;

public interface ICache
{
    void Delete(string key);
    string Get(string key);
    void Put(string key, string value);
    bool TryDelete(string key);
    bool TryGet(string key, out string value);
    long Count { get; }
}

