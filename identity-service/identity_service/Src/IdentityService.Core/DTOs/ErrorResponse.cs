namespace IdentityService.Core.DTOs;

/// <summary>
/// Standard error response format
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error code
    /// </summary>
    public required string ErrorCode { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Error timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Trace ID for correlation
    /// </summary>
    public string? TraceId { get; set; }
}
