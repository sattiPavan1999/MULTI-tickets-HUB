using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using AdminBFF.Core.DTOs;
using AdminBFF.Core.Models;

namespace AdminBFF.Core.Services;

public class IdentityService : IIdentityService
{
    private readonly HttpClient _httpClient;
    private readonly GraphQLHttpClient _graphql;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(HttpClient httpClient, ILogger<IdentityService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _graphql = new GraphQLHttpClient(httpClient, logger);
    }

    public async Task<UserDto> GetUserByIdAsync(int userId)
    {
        const string query = @"
            query($id: Int!) {
                user(id: $id) { id email fullName phoneNumber role createdAt }
            }";
        var user = await _graphql.QueryAsync<UserDto?>(query, new { id = userId }, "user");
        if (user == null)
        {
            throw new NotFoundException($"User with ID {userId} not found");
        }
        return user;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        const string query = @"
            query {
                users { id email fullName phoneNumber role createdAt }
            }";
        return await _graphql.QueryAsync<List<UserDto>>(query, null, "users");
    }

    public async Task<int> GetActiveUserCountAsync()
    {
        const string query = "query { userCount }";
        return await _graphql.QueryAsync<int>(query, null, "userCount");
    }

    public async Task<OperationResultDto> DeactivateUserAsync(int userId)
    {
        try
        {
            var response = await _httpClient.PutAsync($"/api/users/{userId}/deactivate", null);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new NotFoundException($"User with ID {userId} not found");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new ValidationException(await response.Content.ReadAsStringAsync());
                }
                throw new ServiceUnavailableException("Identity Service returned error", new Exception($"Status: {response.StatusCode}"));
            }

            return new OperationResultDto { Success = true, Message = "User deactivated successfully" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Identity Service");
            throw new ServiceUnavailableException("Identity Service unavailable", ex);
        }
    }
}
