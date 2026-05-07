namespace AdminBFF.Core.Configuration;

public class ServiceEndpoints
{
    public required string IdentityServiceUrl { get; init; }
    public required string TrainServiceUrl { get; init; }
    public required string MovieServiceUrl { get; init; }
}
