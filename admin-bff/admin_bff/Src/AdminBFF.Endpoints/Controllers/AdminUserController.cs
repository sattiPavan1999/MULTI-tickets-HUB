using AdminBFF.Core.DTOs;
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
    public async Task<ActionResult<OperationResult>> ToggleUserStatus(int id, CancellationToken ct)
    {
        var token = ExtractToken();
        var result = await identityService.ToggleUserStatusAsync(id, token, ct);
        return Ok(result);
    }

    private string ExtractToken()
    {
        var authHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        return authHeader?.StartsWith("Bearer ") == true ? authHeader[7..] : string.Empty;
    }
}
