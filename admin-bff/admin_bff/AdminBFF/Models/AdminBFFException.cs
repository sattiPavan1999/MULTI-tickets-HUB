namespace AdminBFF.Models;

public class AdminBFFException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    public AdminBFFException(string message, string errorCode, int statusCode)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public AdminBFFException(string message, string errorCode, int statusCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

public class UnauthorizedException : AdminBFFException
{
    public UnauthorizedException(string message)
        : base(message, "UNAUTHORIZED", 401)
    {
    }
}

public class ForbiddenException : AdminBFFException
{
    public ForbiddenException(string message)
        : base(message, "FORBIDDEN", 403)
    {
    }
}

public class NotFoundException : AdminBFFException
{
    public NotFoundException(string message)
        : base(message, "NOT_FOUND", 404)
    {
    }
}

public class ValidationException : AdminBFFException
{
    public ValidationException(string message)
        : base(message, "VALIDATION_ERROR", 400)
    {
    }
}

public class ServiceUnavailableException : AdminBFFException
{
    public ServiceUnavailableException(string message, Exception innerException)
        : base(message, "SERVICE_UNAVAILABLE", 500, innerException)
    {
    }
}
