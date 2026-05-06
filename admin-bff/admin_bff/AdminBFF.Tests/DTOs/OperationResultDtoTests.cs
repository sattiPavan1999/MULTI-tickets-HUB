using AdminBFF.DTOs;

namespace AdminBFF.Tests.DTOs;

public class OperationResultDtoTests
{
    [Fact]
    public void OperationResultDto_Should_Initialize_With_Success()
    {
        // Arrange & Act
        var result = new OperationResultDto
        {
            Success = true,
            Message = "Operation completed successfully"
        };

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Operation completed successfully", result.Message);
    }

    [Fact]
    public void OperationResultDto_Should_Initialize_With_Failure()
    {
        // Arrange & Act
        var result = new OperationResultDto
        {
            Success = false,
            Message = "Operation failed"
        };

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Operation failed", result.Message);
    }

    [Theory]
    [InlineData(true, "Success message")]
    [InlineData(false, "Failure message")]
    public void OperationResultDto_Should_Accept_Various_Values(bool success, string message)
    {
        // Act
        var result = new OperationResultDto
        {
            Success = success,
            Message = message
        };

        // Assert
        Assert.Equal(success, result.Success);
        Assert.Equal(message, result.Message);
    }

    [Fact]
    public void OperationResultDto_Should_Be_Immutable()
    {
        // Arrange
        var original = new OperationResultDto
        {
            Success = true,
            Message = "Original message"
        };

        // Act
        var modified = original with { Message = "Modified message" };

        // Assert
        Assert.Equal("Original message", original.Message);
        Assert.Equal("Modified message", modified.Message);
        Assert.True(original.Success);
        Assert.True(modified.Success);
    }
}
