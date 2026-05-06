namespace IdentityService.Services;

/// <summary>
/// Audit logging service interface
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Log audit event
    /// </summary>
    Task LogAsync(string message);
}
