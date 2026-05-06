using IdentityService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    [HttpPut("{id:int}/deactivate")]
    public ActionResult<OperationResult> Deactivate(int id)
    {
        return Ok(new OperationResult { Success = true, Message = $"User {id} deactivation acknowledged" });
    }
}
