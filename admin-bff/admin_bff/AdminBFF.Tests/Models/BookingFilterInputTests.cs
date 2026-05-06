using AdminBFF.Models;

namespace AdminBFF.Tests.Models;

public class BookingFilterInputTests
{
    [Fact]
    public void BookingFilterInput_Should_Initialize_With_Null_Values()
    {
        // Arrange & Act
        var filter = new BookingFilterInput();

        // Assert
        Assert.Null(filter.Status);
        Assert.Null(filter.ServiceType);
    }

    [Fact]
    public void BookingFilterInput_Should_Initialize_With_Status_Only()
    {
        // Arrange & Act
        var filter = new BookingFilterInput
        {
            Status = "Confirmed"
        };

        // Assert
        Assert.Equal("Confirmed", filter.Status);
        Assert.Null(filter.ServiceType);
    }

    [Fact]
    public void BookingFilterInput_Should_Initialize_With_ServiceType_Only()
    {
        // Arrange & Act
        var filter = new BookingFilterInput
        {
            ServiceType = "Train"
        };

        // Assert
        Assert.Equal("Train", filter.ServiceType);
        Assert.Null(filter.Status);
    }

    [Fact]
    public void BookingFilterInput_Should_Initialize_With_Both_Values()
    {
        // Arrange & Act
        var filter = new BookingFilterInput
        {
            Status = "Cancelled",
            ServiceType = "Movie"
        };

        // Assert
        Assert.Equal("Cancelled", filter.Status);
        Assert.Equal("Movie", filter.ServiceType);
    }

    [Theory]
    [InlineData("Confirmed", "Train")]
    [InlineData("Cancelled", "Movie")]
    [InlineData("Confirmed", "Movie")]
    [InlineData("Cancelled", "Train")]
    public void BookingFilterInput_Should_Accept_Valid_Combinations(string status, string serviceType)
    {
        // Act
        var filter = new BookingFilterInput
        {
            Status = status,
            ServiceType = serviceType
        };

        // Assert
        Assert.Equal(status, filter.Status);
        Assert.Equal(serviceType, filter.ServiceType);
    }
}
