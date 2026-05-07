using TrainService.Core.DTOs;

namespace TrainService.Tests.DTOs;

public class PassengerDetailTests
{
    [Fact]
    public void PassengerDetail_Properties_SetCorrectly()
    {
        // Arrange & Act
        var passenger = new PassengerDetail
        {
            Name = "John Doe",
            Age = 35,
            Gender = "Male"
        };

        // Assert
        Assert.Equal("John Doe", passenger.Name);
        Assert.Equal(35, passenger.Age);
        Assert.Equal("Male", passenger.Gender);
    }

    [Theory]
    [InlineData("Jane Smith", 28, "Female")]
    [InlineData("Alex Johnson", 45, "Other")]
    [InlineData("Child Passenger", 5, "Male")]
    public void PassengerDetail_ValidData_Theory(string name, int age, string gender)
    {
        // Arrange & Act
        var passenger = new PassengerDetail
        {
            Name = name,
            Age = age,
            Gender = gender
        };

        // Assert
        Assert.Equal(name, passenger.Name);
        Assert.Equal(age, passenger.Age);
        Assert.Equal(gender, passenger.Gender);
    }
}
