using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityService.Core.DTOs;
using IdentityService.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Endpoints.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserType>> Register([FromBody] RegisterInput input, CancellationToken ct)
    {
        var user = await authService.RegisterAsync(input, ct);
        return CreatedAtAction(null, new { id = user.Id }, user);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginInput input, CancellationToken ct)
    {
        var response = await authService.LoginAsync(input, ct);
        return Ok(response);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<UserType>> UpdateProfile([FromBody] UpdateProfileInput input, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await authService.UpdateProfileAsync(userId, input, ct);
        return Ok(user);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordInput input, CancellationToken ct)
    {
        var response = await authService.ForgotPasswordAsync(input, ct);
        return Ok(response);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResult>> ResetPassword([FromBody] ResetPasswordInput input, CancellationToken ct)
    {
        var response = await authService.ResetPasswordAsync(input, ct);
        return Ok(response);
    }
}
