namespace AdminBFF.DTOs;

public record UserDto
{
    public int Id { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Role { get; init; }
    public DateTime CreatedAt { get; init; }
}
