using System.Net.Http.Json;
using AdminBFF.Core.DTOs;
using Microsoft.Extensions.Logging;

namespace AdminBFF.Core.Services;

public class IdentityServiceClient(HttpClient httpClient, ILogger<IdentityServiceClient> logger) : IIdentityService
{
    public async Task<List<UserDto>> GetAllUsersAsync(string bearerToken, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/users");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>(ct);
        return users ?? [];
    }

    public async Task<OperationResult> ToggleUserStatusAsync(int userId, string bearerToken, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/auth/users/{userId}/toggle-status");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OperationResult>(ct);
        return result ?? new OperationResult { Success = true };
    }
}
