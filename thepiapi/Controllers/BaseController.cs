using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace thepiapi.Controllers;

[Authorize]
[ApiController]
public abstract class BaseController : ControllerBase
{
    // This property is now available to all child controllers
    protected int UserId => GetUserId();

    protected int GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out int id) ? id : 0;
    }
}