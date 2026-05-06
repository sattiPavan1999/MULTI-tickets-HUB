using IdentityService.Models.DTOs;

namespace IdentityService.Tests.Models;

public class ErrorResponseTests
{
    [Fact]
    public void ErrorResponse_WithValidData_CreatesInstance()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var traceId = Guid.NewGuid().ToString();

        // Act
        var errorResponse = new ErrorResponse
        {
            ErrorCode = "UNAUTHORIZED",
            Message = "Invalid credentials",
            Timestamp = timestamp,
            TraceId = traceId
        };

        // Assert
        Assert.Equal("UNAUTHORIZED", errorResponse.ErrorCode);
        Assert.Equal("Invalid credentials", errorResponse.Message);
        Assert.Equal(timestamp, errorResponse.Timestamp);
        Assert.Equal(traceId, errorResponse.TraceId);
    }

    [Fact]
    public void ErrorResponse_TraceIdNullable_AcceptsNull()
    {
        // Arrange & Act
        var errorResponse = new ErrorResponse
        {
            ErrorCode = "INTERNAL_ERROR",
            Message = "An error occurred",
            Timestamp = DateTime.UtcNow,
            TraceId = null
        };

        // Assert
        Assert.Null(errorResponse.TraceId);
    }

    [Theory]
    [InlineData("UNAUTHORIZED", "Invalid credentials")]
    [InlineData("EMAIL_EXISTS", "Email already registered")]
    [InlineData("NOT_FOUND", "User not found")]
    [InlineData("VALIDATION_ERROR", "Validation failed")]
    [InlineData("INTERNAL_ERROR", "Internal server error")]
    public void ErrorResponse_CommonErrorCodes_StoresCorrectly(string errorCode, string message)
    {
        // Arrange & Act
        var errorResponse = new ErrorResponse
        {
            ErrorCode = errorCode,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(errorCode, errorResponse.ErrorCode);
        Assert.Equal(message, errorResponse.Message);
    }
}
