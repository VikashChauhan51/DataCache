using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCache.EvictionStrategies;

public interface IEvictionStrategy<TKey>
{
    void AccessItem(TKey key);
    void AddItem(TKey key);
    void RemoveItem(TKey key);
    TKey EvictItem();
}
