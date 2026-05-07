using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AdminBFF.Core.Models;

namespace AdminBFF.Core.Services;

public class GraphQLHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GraphQLHttpClient(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<T> QueryAsync<T>(string query, object? variables = null, string fieldName = "")
    {
        try
        {
            var request = new { query, variables };
            var response = await _httpClient.PostAsJsonAsync("/graphql", request, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                throw new ServiceUnavailableException(
                    $"Downstream GraphQL returned {(int)response.StatusCode}",
                    new Exception(await response.Content.ReadAsStringAsync()));
            }

            var raw = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

            if (raw.TryGetProperty("errors", out var errors) && errors.GetArrayLength() > 0)
            {
                var msg = errors[0].TryGetProperty("message", out var m) ? m.GetString() : "GraphQL error";
                throw new ServiceUnavailableException(msg ?? "GraphQL error", new Exception(errors.ToString()));
            }

            if (!raw.TryGetProperty("data", out var data))
            {
                throw new ServiceUnavailableException("GraphQL response missing data", new Exception(raw.ToString()));
            }

            var payload = string.IsNullOrEmpty(fieldName) ? data : data.GetProperty(fieldName);
            var result = JsonSerializer.Deserialize<T>(payload.GetRawText(), JsonOptions);
            return result ?? throw new ServiceUnavailableException("Null GraphQL payload", new Exception(payload.ToString()));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GraphQL request failed");
            throw new ServiceUnavailableException("Downstream service unavailable", ex);
        }
    }
}
