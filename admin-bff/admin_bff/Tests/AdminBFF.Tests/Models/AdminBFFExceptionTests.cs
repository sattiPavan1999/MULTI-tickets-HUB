using AdminBFF.Core.Models;

namespace AdminBFF.Tests.Models;

public class AdminBFFExceptionTests
{
    [Fact]
    public void AdminBFFException_Should_Initialize_With_Message_And_Codes()
    {
        // Arrange & Act
        var exception = new AdminBFFException("Test message", "TEST_ERROR", 400);

        // Assert
        Assert.Equal("Test message", exception.Message);
        Assert.Equal("TEST_ERROR", exception.ErrorCode);
        Assert.Equal(400, exception.StatusCode);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void AdminBFFException_Should_Initialize_With_InnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new AdminBFFException("Test message", "TEST_ERROR", 500, innerException);

        // Assert
        Assert.Equal("Test message", exception.Message);
        Assert.Equal("TEST_ERROR", exception.ErrorCode);
        Assert.Equal(500, exception.StatusCode);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void UnauthorizedException_Should_Have_Correct_Status_And_Code()
    {
        // Arrange & Act
        var exception = new UnauthorizedException("Invalid token");

        // Assert
        Assert.Equal("Invalid token", exception.Message);
        Assert.Equal("UNAUTHORIZED", exception.ErrorCode);
        Assert.Equal(401, exception.StatusCode);
    }

    [Fact]
    public void ForbiddenException_Should_Have_Correct_Status_And_Code()
    {
        // Arrange & Act
        var exception = new ForbiddenException("Admin access required");

        // Assert
        Assert.Equal("Admin access required", exception.Message);
        Assert.Equal("FORBIDDEN", exception.ErrorCode);
        Assert.Equal(403, exception.StatusCode);
    }

    [Fact]
    public void NotFoundException_Should_Have_Correct_Status_And_Code()
    {
        // Arrange & Act
        var exception = new NotFoundException("User not found");

        // Assert
        Assert.Equal("User not found", exception.Message);
        Assert.Equal("NOT_FOUND", exception.ErrorCode);
        Assert.Equal(404, exception.StatusCode);
    }

    [Fact]
    public void ValidationException_Should_Have_Correct_Status_And_Code()
    {
        // Arrange & Act
        var exception = new ValidationException("Invalid input");

        // Assert
        Assert.Equal("Invalid input", exception.Message);
        Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public void ServiceUnavailableException_Should_Have_Correct_Status_And_Code()
    {
        // Arrange
        var innerException = new HttpRequestException("Connection failed");

        // Act
        var exception = new ServiceUnavailableException("Identity Service unavailable", innerException);

        // Assert
        Assert.Equal("Identity Service unavailable", exception.Message);
        Assert.Equal("SERVICE_UNAVAILABLE", exception.ErrorCode);
        Assert.Equal(500, exception.StatusCode);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Theory]
    [InlineData("UNAUTHORIZED", 401)]
    [InlineData("FORBIDDEN", 403)]
    [InlineData("NOT_FOUND", 404)]
    [InlineData("VALIDATION_ERROR", 400)]
    [InlineData("SERVICE_UNAVAILABLE", 500)]
    public void Exception_ErrorCode_Should_Match_StatusCode_Convention(string errorCode, int statusCode)
    {
        // Act
        var exception = new AdminBFFException("Test", errorCode, statusCode);

        // Assert
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Equal(statusCode, exception.StatusCode);
    }

    [Fact]
    public void All_Custom_Exceptions_Should_Be_AdminBFFException()
    {
        // Assert
        Assert.IsAssignableFrom<AdminBFFException>(new UnauthorizedException("test"));
        Assert.IsAssignableFrom<AdminBFFException>(new ForbiddenException("test"));
        Assert.IsAssignableFrom<AdminBFFException>(new NotFoundException("test"));
        Assert.IsAssignableFrom<AdminBFFException>(new ValidationException("test"));
        Assert.IsAssignableFrom<AdminBFFException>(new ServiceUnavailableException("test", new Exception()));
    }

    [Fact]
    public void All_Custom_Exceptions_Should_Be_System_Exception()
    {
        // Assert
        Assert.IsAssignableFrom<Exception>(new AdminBFFException("test", "TEST", 500));
        Assert.IsAssignableFrom<Exception>(new UnauthorizedException("test"));
        Assert.IsAssignableFrom<Exception>(new ForbiddenException("test"));
        Assert.IsAssignableFrom<Exception>(new NotFoundException("test"));
        Assert.IsAssignableFrom<Exception>(new ValidationException("test"));
        Assert.IsAssignableFrom<Exception>(new ServiceUnavailableException("test", new Exception()));
    }
}
