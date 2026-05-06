namespace AdminBFF.DTOs;

public record OperationResultDto
{
    public bool Success { get; init; }
    public required string Message { get; init; }
}
