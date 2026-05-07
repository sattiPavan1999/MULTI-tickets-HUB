using AdminBFF.Core.DTOs;

namespace AdminBFF.Tests.DTOs;

public class DashboardStatsDtoTests
{
    [Fact]
    public void DashboardStatsDto_Should_Initialize_With_Valid_Values()
    {
        // Arrange & Act
        var stats = new DashboardStatsDto
        {
            TotalBookings = 1523,
            ActiveUsers = 487,
            CancellationCount = 89
        };

        // Assert
        Assert.Equal(1523, stats.TotalBookings);
        Assert.Equal(487, stats.ActiveUsers);
        Assert.Equal(89, stats.CancellationCount);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(100, 50, 10)]
    [InlineData(999999, 999999, 999999)]
    public void DashboardStatsDto_Should_Accept_Various_Valid_Values(int totalBookings, int activeUsers, int cancellationCount)
    {
        // Act
        var stats = new DashboardStatsDto
        {
            TotalBookings = totalBookings,
            ActiveUsers = activeUsers,
            CancellationCount = cancellationCount
        };

        // Assert
        Assert.Equal(totalBookings, stats.TotalBookings);
        Assert.Equal(activeUsers, stats.ActiveUsers);
        Assert.Equal(cancellationCount, stats.CancellationCount);
    }

    [Fact]
    public void DashboardStatsDto_Should_Be_Immutable()
    {
        // Arrange
        var original = new DashboardStatsDto
        {
            TotalBookings = 100,
            ActiveUsers = 50,
            CancellationCount = 10
        };

        // Act
        var modified = original with { TotalBookings = 200 };

        // Assert
        Assert.Equal(100, original.TotalBookings);
        Assert.Equal(200, modified.TotalBookings);
        Assert.Equal(original.ActiveUsers, modified.ActiveUsers);
    }
}
