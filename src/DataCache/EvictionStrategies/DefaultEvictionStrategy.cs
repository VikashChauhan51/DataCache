namespace DataCache.EvictionStrategies;

public class DefaultEvictionStrategy : IEvictionStrategy<string>
{
    public void AccessItem(string key)
    {
        
    }

    public void AddItem(string key)
    {
        
    }

    public string EvictItem()
    {
         return default!;
    }

    public void RemoveItem(string key)
    {
        
    }
}
