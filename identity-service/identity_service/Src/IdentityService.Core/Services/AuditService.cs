using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace IdentityService.Core.Services;

/// <summary>
/// Audit logging service implementation
/// </summary>
public class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;

    public AuditService(ILogger<AuditService> logger)
    {
        _logger = logger;
    }

    public Task LogAsync(string message)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? "N/A";

        _logger.LogInformation("[AUDIT] TraceId: {TraceId} - {Message}", traceId, message);

        return Task.CompletedTask;
    }
}
