using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AdminBFF.Core.DTOs;
using AdminBFF.Core.Exceptions;
using AdminBFF.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminBFF.Endpoints.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUserController(IIdentityService identityService, IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    [HttpPut("{id:int}/toggle-status")]
    public async Task<ActionResult<OperationResult>> ToggleUserStatus(int id)
    {
        var callerIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (callerIdClaim is not null && int.TryParse(callerIdClaim, out var callerId) && callerId == id)
            throw new ProxyException(409, "Administrators cannot toggle their own account status");

        var token = ExtractToken();
        var result = await identityService.ToggleUserStatusAsync(id, token);
        return Ok(result);
    }

    private string ExtractToken()
    {
        var authHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        return authHeader?.StartsWith("Bearer ") == true ? authHeader[7..] : string.Empty;
    }
}
