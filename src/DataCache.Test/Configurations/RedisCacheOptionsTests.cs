using DataCache.Cache;
using DataCache.Configurations;

namespace DataCache.Test.Configurations;

[Trait("Category", "Unit")]
public class RedisCacheOptionsTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        long expectedMaxMemorySize = 2048;
        Eviction expectedEvictionType = Eviction.LRU;
        TimeSpan expectedTtlInterval = TimeSpan.FromMinutes(10);
        int expectedDatabaseIndex = 2;
        bool expectedOptimized = true;

        // Act
        var options = new RedisCacheOptions(
            expectedMaxMemorySize,
            expectedEvictionType,
            expectedTtlInterval,
            expectedDatabaseIndex,
            expectedOptimized);

        // Assert
        options.MaxMemorySize.Should().Be(expectedMaxMemorySize);
        options.EvictionType.Should().Be(expectedEvictionType);
        options.TtlInterval.Should().Be(expectedTtlInterval);
        options.DatabaseIndex.Should().Be(expectedDatabaseIndex);
        options.Optimized.Should().Be(expectedOptimized);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenInvalidDatabaseIndex()
    {
        // Act
        Action act = () => new RedisCacheOptions(
            1024,
            Eviction.None,
            TimeSpan.FromMinutes(5),
            -1,
            true);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("DatabaseIndex cannot be negative*");
    }

}

