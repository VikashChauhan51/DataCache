
using Bogus;
using DataCache.Cache;
using DataCache.Configurations;

namespace DataCache.Test.Configurations;

[Trait("Category", "Unit")]
public class CacheOptionsTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {

        // Act
        var cacheOptions = new CacheOptions(
            Randomizer.Seed.Next(1, 1000000),
            Eviction.None, new TimeSpan(0,
            Randomizer.Seed.Next(1, 5), 0, 0, 0));

        // Assert
        cacheOptions.MaxMemorySize.Should().BeGreaterThan(0);
        cacheOptions.TtlInterval.Should().BePositive();
        Enum.IsDefined(typeof(Cache.Eviction), cacheOptions.EvictionType).Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenNegativeMaxMemorySize()
    {
        // Act
        Action act = () => new CacheOptions(-1, Eviction.LRU, TimeSpan.FromMinutes(5));

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("MaxMemorySize cannot be negative*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenInvalidEvictionType()
    {
        // Act
        Action act = () => new CacheOptions(1024, (Eviction)999, TimeSpan.FromMinutes(5));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Invalid eviction type*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenNegativeTtlInterval()
    {
        // Act
        Action act = () => new CacheOptions(1024, Eviction.LRU, TimeSpan.FromSeconds(-5));

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("TTL interval cannot be negative*");
    }
}
