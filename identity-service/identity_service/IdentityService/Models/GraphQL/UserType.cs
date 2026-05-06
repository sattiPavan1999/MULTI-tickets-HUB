namespace IdentityService.Models.GraphQL;

/// <summary>
/// GraphQL user type
/// </summary>
public class UserType
{
    /// <summary>
    /// User's unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User's email address
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's full name
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// User's phone number
    /// </summary>
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// User's role
    /// </summary>
    public required string Role { get; set; }

    /// <summary>
    /// Account creation timestamp
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
