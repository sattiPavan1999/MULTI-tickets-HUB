using System.Net.Http.Json;
using AdminBFF.Core.DTOs;
using AdminBFF.Core.Exceptions;

namespace AdminBFF.Core.Services;

public class IdentityServiceClient(HttpClient httpClient) : IIdentityService
{
    private static async Task ThrowIfErrorAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        ErrorResponse? body = null;
        try { body = await response.Content.ReadFromJsonAsync<ErrorResponse>(); } catch { }
        var message = body?.Message ?? response.ReasonPhrase ?? "Upstream request failed";
        throw new ProxyException((int)response.StatusCode, message);
    }

    public async Task<List<UserDto>> GetAllUsersAsync(string bearerToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/users");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

        var response = await httpClient.SendAsync(request);
        await ThrowIfErrorAsync(response);

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        return users ?? [];
    }

    public async Task<OperationResult> ToggleUserStatusAsync(int userId, string bearerToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/auth/users/{userId}/toggle-status");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

        var response = await httpClient.SendAsync(request);
        await ThrowIfErrorAsync(response);

        var result = await response.Content.ReadFromJsonAsync<OperationResult>();
        return result ?? new OperationResult { Success = true };
    }
}
