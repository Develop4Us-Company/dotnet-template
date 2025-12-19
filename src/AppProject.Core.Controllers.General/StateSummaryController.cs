using AppProject.Core.Models.General;
using AppProject.Core.Services.General;
using AppProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.Controllers.General
{
    [Route("api/general/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class StateSummaryController(IStateSummaryService stateSummaryService)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetSummariesAsync([FromQuery] StateSummarySearchRequest request, CancellationToken cancellationToken = default)
        {
            return this.Ok(await stateSummaryService.GetSummariesAsync(request, cancellationToken));
        }

        [HttpGet]
        public async Task<IActionResult> GetSummaryAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
        {
            return this.Ok(await stateSummaryService.GetSummaryAsync(request, cancellationToken));
        }
    }
}
