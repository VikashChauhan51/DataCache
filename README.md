
# DataCache

DataCache is a flexible, high-performance caching library designed for both in-memory and Redis-based caching scenarios. It supports various eviction strategies, including LRU (Least Recently Used), LFU (Least Frequently Used), and MRU (Most Recently Used), to handle cache eviction efficiently. The library also allows for configurable memory size limits, Time-To-Live (TTL) management, and provides a seamless integration between Redis and in-memory caching for optimized performance.

## Features

- **Eviction Strategies**: Supports multiple eviction strategies such as LRU, LFU, MRU, and more.
- **Memory Size Limit**: Configurable memory size limits for in-memory caches.
- **TTL (Time-To-Live) Management**: Auto-removal of expired items based on TTL settings.
- **Redis Integration**: Provides a Redis-based caching option with support for database selection and optimization through an optional in-memory cache.
- **Thread-Safe Operations**: Ensures thread safety for concurrent cache access.
- **Modular Design**: Extensible design allowing custom cache implementations and eviction strategies.

## Installation

1. Clone the repository:
    ```bash
    git clone https://github.com/VikashChauhan51/DataCache.git
    cd DataCache
    ```

2. Add a reference to the project or use NuGet to install the package:
    ```bash
    dotnet add package DataCache --version x.y.z
    ```

## Usage

### 1. Basic In-Memory Cache Example

```csharp
var cacheOptions = new CacheOptions
{
    MaxMemorySize = 1048576, // 1 MB
    EvictionType = Eviction.LRU,
    TtlInterval = TimeSpan.FromMinutes(5)
};

var memoryCache = new MemoryCache(cacheOptions);

// Add an item
memoryCache.Put("item1", new CacheItem("Value1", DateTimeOffset.Now, TimeSpan.FromMinutes(10)));

// Get an item
var item = memoryCache.Get("item1");

// Check item expiration
if (item?.IsExpired ?? false)
{
    memoryCache.Delete("item1");
}
```

### 2. Redis Cache Example with Optimized In-Memory Caching

```csharp
var redisCacheOptions = new RedisCacheOptions(
    maxMemorySize: 2097152,  // 2 MB
    evictionType: Eviction.LFU,
    TtlInterval: TimeSpan.FromMinutes(10),
    databaseIndex: 0,
    optimized: true
);

var redisConnection = ConnectionMultiplexer.Connect("localhost");
var redisCache = new RedisCache(redisCacheOptions, redisConnection);

// Add an item
await redisCache.PutAsync("user:1", new CacheItem("User Data", DateTimeOffset.Now, TimeSpan.FromMinutes(30)));

// Get an item
var userData = await redisCache.GetAsync("user:1");
```

### 3. Custom Eviction Strategy

To implement your own eviction strategy, you can extend the `IEvictionStrategy` interface and plug it into the cache configuration.

```csharp
public class CustomEvictionStrategy<TKey> : IEvictionStrategy<TKey>
{
    public void RecordAccess(TKey key) { /* Custom logic */ }
    public void RecordInsertion(TKey key) { /* Custom logic */ }
    public TKey? Evict() { /* Custom eviction logic */ }
}

// Usage
var cacheOptions = new CacheOptions
{
    MaxMemorySize = 1048576, // 1 MB
    EvictionType = Eviction.None, // Use your custom strategy
};

var customEvictionStrategy = new CustomEvictionStrategy<string>();
var memoryCache = new MemoryCache(cacheOptions, customEvictionStrategy);
```

## Configuration

The cache options allow full customization based on your needs:

- **MaxMemorySize**: Limits the memory size used for in-memory caching.
- **EvictionType**: Specify which eviction strategy to use. Choose from `Eviction.LRU`, `Eviction.LFU`, `Eviction.MRU`, and others.
- **TtlInterval**: Interval for TTL checks to clean up expired items.
- **Redis Options**:
  - **DatabaseIndex**: Specify which Redis database to use.
  - **Optimized**: Enable/disable in-memory caching for performance optimization.

## Contributing

We welcome contributions! Please feel free to open issues, submit PRs, and suggest new features. Make sure to write unit tests for your contributions using xUnit, FluentAssertions, and FakeItEasy.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

This template covers the basics of explaining your library, how to use it, and how to contribute. You can modify this as necessary based on your project's specific needs.
