using TrainService.DTOs;

namespace TrainService.Tests.DTOs;

public class CreateBookingInputTests
{
    [Fact]
    public void CreateBookingInput_Properties_SetCorrectly()
    {
        // Arrange
        var passengerDetails = new List<PassengerDetail>
        {
            new PassengerDetail { Name = "John Doe", Age = 35, Gender = "Male" }
        };

        // Act
        var input = new CreateBookingInput
        {
            UserId = 42,
            TrainId = 1,
            TravelDate = new DateOnly(2026, 6, 15),
            SeatClass = "2AC",
            NumberOfPassengers = 1,
            PassengerDetails = passengerDetails
        };

        // Assert
        Assert.Equal(42, input.UserId);
        Assert.Equal(1, input.TrainId);
        Assert.Equal(new DateOnly(2026, 6, 15), input.TravelDate);
        Assert.Equal("2AC", input.SeatClass);
        Assert.Equal(1, input.NumberOfPassengers);
        Assert.Single(input.PassengerDetails);
    }

    [Fact]
    public void CreateBookingInput_DefaultValues()
    {
        // Arrange & Act
        var input = new CreateBookingInput();

        // Assert
        Assert.Equal(0, input.UserId);
        Assert.Equal(0, input.TrainId);
        Assert.Equal(default, input.TravelDate);
        Assert.Equal(string.Empty, input.SeatClass);
        Assert.Equal(0, input.NumberOfPassengers);
        Assert.NotNull(input.PassengerDetails);
        Assert.Empty(input.PassengerDetails);
    }
}
