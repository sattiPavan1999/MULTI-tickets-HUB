using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ApiGateway.Models;

namespace ApiGateway.Middleware;

public class JwtValidationMiddleware
{
    private const string UserIdHeader = "X-User-Id";

    private readonly RequestDelegate _next;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtValidationMiddleware> _logger;

    public JwtValidationMiddleware(
        RequestDelegate next,
        JwtSettings jwtSettings,
        ILogger<JwtValidationMiddleware> logger)
    {
        _next = next;
        _jwtSettings = jwtSettings;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // Explicitly public — no token required
        if (path == "/api/auth/login" ||
            path == "/api/auth/register" ||
            path == "/api/auth/forgot-password" ||
            path == "/api/auth/reset-password" ||
            path.StartsWith("/health") ||
            path == "/")
        {
            await _next(context);
            return;
        }

        // All other routes require a valid JWT
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader))
        {
            _logger.LogWarning("Authorization header missing for path: {Path}", path);
            await WriteErrorAsync(context, StatusCodes.Status401Unauthorized, "UNAUTHORIZED", "Authorization header missing");
            return;
        }

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Invalid Authorization header format for path: {Path}", path);
            await WriteErrorAsync(context, StatusCodes.Status401Unauthorized, "UNAUTHORIZED", "Invalid token format");
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();

        if (!ValidateToken(token, out var validationError, out var role, out var userId))
        {
            _logger.LogWarning("JWT validation failed for path: {Path}. Error: {Error}", path, validationError);
            await WriteErrorAsync(context, StatusCodes.Status401Unauthorized, "UNAUTHORIZED", validationError);
            return;
        }

        // Inject the authenticated user's ID as a trusted internal header
        if (!string.IsNullOrEmpty(userId))
            context.Request.Headers[UserIdHeader] = userId;

        // Admin paths require the Admin role
        if (path.StartsWith("/graphql/admin") || path.StartsWith("/api/admin"))
        {
            if (!string.Equals(role, Roles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Insufficient permissions for admin route. User role: {Role}", role);
                await WriteErrorAsync(context, StatusCodes.Status403Forbidden, "FORBIDDEN", "Insufficient permissions");
                return;
            }
        }

        _logger.LogInformation("JWT validation successful for path: {Path}", path);
        await _next(context);
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string errorCode, string message)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new
        {
            errorCode,
            message,
            timestamp = DateTime.UtcNow,
            traceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier
        });
    }

    private bool ValidateToken(string token, out string errorMessage, out string? role, out string? userId)
    {
        errorMessage = string.Empty;
        role = null;
        userId = null;

        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                errorMessage = "Invalid token format";
                return false;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

            role = principal.Claims.FirstOrDefault(c => c.Type == "role")?.Value
                ?? principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            userId = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value
                ?? principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            return true;
        }
        catch (SecurityTokenExpiredException)
        {
            errorMessage = "Token has expired";
            return false;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            errorMessage = "Invalid token signature";
            return false;
        }
        catch (SecurityTokenException)
        {
            errorMessage = "Token validation failed";
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            errorMessage = "Token validation failed";
            return false;
        }
    }
}
