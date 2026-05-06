using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityService.Models.DTOs;
using IdentityService.Models.GraphQL;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserType>> Register([FromBody] RegisterInput input)
    {
        var user = await _authService.RegisterAsync(input);
        return CreatedAtAction(null, new { id = user.Id }, user);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginInput input)
    {
        var response = await _authService.LoginAsync(input);
        return Ok(response);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<UserType>> UpdateProfile([FromBody] UpdateProfileInput input)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.UpdateProfileAsync(userId, input);
        return Ok(user);
    }
}
