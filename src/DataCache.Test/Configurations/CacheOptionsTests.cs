
using Bogus;
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
            TimeSpan.FromMinutes(5));

        // Assert
        cacheOptions.MaxMemorySize.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenNegativeMaxMemorySize()
    {
        // Act
        Action act = () => new CacheOptions(-1, TimeSpan.FromMinutes(5));

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("MaxMemorySize cannot be negative*");
    }


    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenNegativeTtlInterval()
    {
        // Act
        Action act = () => new CacheOptions(1024, TimeSpan.FromSeconds(-5));

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("TTL interval cannot be negative*");
    }
}
