using AppProject.Core.Services.Auth;
using AppProject.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.API.Controllers.Auth;

[Route("api/auth/[controller]/[action]")]
[ApiController]
[Authorize]
public class PermissionController(IPermissionService permissionService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCurrentUserPermissionsAsync([FromQuery] PermissionContext? context = null, CancellationToken cancellationToken = default)
    {
        var permissions = await permissionService.GetCurrentUserPermissionsAsync(context, cancellationToken);
        return this.Ok(permissions);
    }
}