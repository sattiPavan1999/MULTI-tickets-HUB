using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ApiGateway.Models;

namespace ApiGateway.Middleware;

public class JwtValidationMiddleware
{
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

        // Skip JWT validation for public routes and health endpoints
        if (path.StartsWith("/graphql/auth") ||
            path == "/api/auth/login" ||
            path == "/api/auth/register" ||
            path == "/api/auth/forgot-password" ||
            path == "/api/auth/reset-password" ||
            path.StartsWith("/health") ||
            path == "/")
        {
            await _next(context);
            return;
        }

        // Protected routes require JWT validation
        if (path.StartsWith("/graphql/trains") ||
            path.StartsWith("/graphql/movies") ||
            path.StartsWith("/graphql/admin"))
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader))
            {
                _logger.LogWarning("Authorization header missing for path: {Path}", path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    errorCode = "UNAUTHORIZED",
                    message = "Authorization header missing",
                    timestamp = DateTime.UtcNow,
                    traceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier
                });
                return;
            }

            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid Authorization header format for path: {Path}", path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    errorCode = "UNAUTHORIZED",
                    message = "Invalid token format",
                    timestamp = DateTime.UtcNow,
                    traceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier
                });
                return;
            }

            var token = authHeader["Bearer ".Length..].Trim();

            if (!ValidateToken(token, out var validationError, out var role))
            {
                _logger.LogWarning("JWT validation failed for path: {Path}. Error: {Error}", path, validationError);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    errorCode = "UNAUTHORIZED",
                    message = validationError,
                    timestamp = DateTime.UtcNow,
                    traceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier
                });
                return;
            }

            // Check role for admin routes
            if (path.StartsWith("/graphql/admin"))
            {
                if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Insufficient permissions for admin route. User role: {Role}", role);
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        errorCode = "FORBIDDEN",
                        message = "Insufficient permissions",
                        timestamp = DateTime.UtcNow,
                        traceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier
                    });
                    return;
                }
            }

            _logger.LogInformation("JWT validation successful for path: {Path}", path);
        }

        await _next(context);
    }

    private bool ValidateToken(string token, out string errorMessage, out string? role)
    {
        errorMessage = string.Empty;
        role = null;

        try
        {
            // Check token format (should have 3 parts separated by dots)
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

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Extract role claim (try both "role" and ClaimTypes.Role)
            role = principal.Claims.FirstOrDefault(c => c.Type == "role")?.Value
                ?? principal.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

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
