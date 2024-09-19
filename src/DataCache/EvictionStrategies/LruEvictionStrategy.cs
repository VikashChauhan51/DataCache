﻿using DataCache.Abstraction;
using DataCache.Configurations;
using System.Collections.Concurrent;

namespace DataCache.EvictionStrategies;


/// <summary>
/// The LRU (Least Recently Used) strategy evicts the least recently accessed items.
/// </summary>
public class LruEvictionStrategy<TKey> : IEvictionStrategy<TKey> where TKey : notnull, IEquatable<TKey>
{
    private readonly LinkedList<TKey> _accessOrder = new();
    private readonly ConcurrentDictionary<TKey, LinkedListNode<TKey>> _keyNodes = new();
    private readonly object _lock = new();

    private readonly long maxSize;

    public LruEvictionStrategy(CacheOptions cacheOptions)
    {
        this.maxSize = cacheOptions.MaxMemorySize;
    }

    /// <inheritdoc />
    public long MaxSize => this.maxSize;

    /// <inheritdoc />
    public long CurrentSize { get; private set; }

    /// <inheritdoc />
    public void OnItemAdded(TKey key)
    {
        var node = new LinkedListNode<TKey>(key);
        if (_keyNodes.TryAdd(key, node))
        {
            lock (_lock)
            {
                _accessOrder.AddLast(node);
            }
        }
    }


    /// <inheritdoc />
    public void OnItemAccessed(TKey key)
    {
        if (_keyNodes.TryGetValue(key, out var node))
        {
            lock (_lock)
            {
                _accessOrder.Remove(node);
                _accessOrder.AddLast(node);
            }
        }
    }


    /// <inheritdoc />
    public void OnItemRemoved(TKey key, long size)
    {
        if (_keyNodes.TryRemove(key, out var node))
        {
            lock (_lock)
            {
                _accessOrder.Remove(node);
            }
            this.CurrentSize -= size;
        }
    }

    /// <inheritdoc />
    public TKey GetEvictionKey()
    {
        lock (_lock)
        {
            var oldest = _accessOrder.First;
            if (oldest != null)
            {
                _accessOrder.RemoveFirst();
                _keyNodes.TryRemove(oldest.Value, out _);
                return oldest.Value;
            }
        }
        return default!;
    }
}

