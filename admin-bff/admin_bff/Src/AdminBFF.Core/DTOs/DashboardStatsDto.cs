namespace AdminBFF.Core.DTOs;

public record DashboardStatsDto
{
    public int TotalBookings { get; init; }
    public int ActiveUsers { get; init; }
    public int CancellationCount { get; init; }
}
