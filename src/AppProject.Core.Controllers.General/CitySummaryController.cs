using System;
using AppProject.Core.Models.General;
using AppProject.Core.Services.General;
using AppProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.Controllers.General;

[Route("api/general/[controller]/[action]")]
[ApiController]
[Authorize]
public class CitySummaryController(ICitySummaryService citySummaryService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSummariesAsync([FromQuery] CitySummarySearchRequest request, CancellationToken cancellationToken = default)
    {
        return this.Ok(await citySummaryService.GetSummariesAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetSummaryAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
    {
        return this.Ok(await citySummaryService.GetSummaryAsync(request, cancellationToken));
    }
}
