using TrainService.Core.Models;

namespace TrainService.Tests.Models;

public class EnumTests
{
    [Fact]
    public void SeatClass_AllValuesPresent()
    {
        // Act
        var values = Enum.GetValues<SeatClass>();

        // Assert
        Assert.Contains(SeatClass.Sleeper, values);
        Assert.Contains(SeatClass.AC3Tier, values);
        Assert.Contains(SeatClass.AC2Tier, values);
        Assert.Contains(SeatClass.AC1Tier, values);
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void BookingStatus_AllValuesPresent()
    {
        // Act
        var values = Enum.GetValues<BookingStatus>();

        // Assert
        Assert.Contains(BookingStatus.Confirmed, values);
        Assert.Contains(BookingStatus.Cancelled, values);
        Assert.Equal(2, values.Length);
    }

    [Theory]
    [InlineData(SeatClass.Sleeper, "Sleeper")]
    [InlineData(SeatClass.AC3Tier, "AC3Tier")]
    [InlineData(SeatClass.AC2Tier, "AC2Tier")]
    [InlineData(SeatClass.AC1Tier, "AC1Tier")]
    public void SeatClass_ToStringCorrect(SeatClass seatClass, string expected)
    {
        // Act
        var result = seatClass.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(BookingStatus.Confirmed, "Confirmed")]
    [InlineData(BookingStatus.Cancelled, "Cancelled")]
    public void BookingStatus_ToStringCorrect(BookingStatus status, string expected)
    {
        // Act
        var result = status.ToString();

        // Assert
        Assert.Equal(expected, result);
    }
}
