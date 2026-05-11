using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityService.Core.DTOs;
using IdentityService.Core.Exceptions;
using IdentityService.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IdentityService.Endpoints.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    IUserAccountService userAccountService,
    IPasswordService passwordService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserType>> Register([FromBody] RegisterInput input)
    {
        var user = await authService.RegisterAsync(input);
        return CreatedAtAction(null, new { id = user.Id }, user);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginInput input)
    {
        var response = await authService.LoginAsync(input);
        return Ok(response);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<UserType>> UpdateProfile([FromBody] UpdateProfileInput input)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await userAccountService.UpdateProfileAsync(userId, input);
        return Ok(user);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("password-reset")]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordInput input)
    {
        var response = await passwordService.ForgotPasswordAsync(input);
        return Ok(response);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("password-reset")]
    public async Task<ActionResult<OperationResult>> ResetPassword([FromBody] ResetPasswordInput input)
    {
        var response = await passwordService.ResetPasswordAsync(input);
        return Ok(response);
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UserType>>> GetAllUsers()
    {
        var users = await userAccountService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPut("users/{id:int}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OperationResult>> ToggleUserStatus(int id)
    {
        var callerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (callerIdClaim is not null && int.TryParse(callerIdClaim, out var callerId) && callerId == id)
            throw new ConflictException("Administrators cannot toggle their own account status");

        var result = await userAccountService.ToggleUserStatusAsync(id);
        return Ok(result);
    }
}
