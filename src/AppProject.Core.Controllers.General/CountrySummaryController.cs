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
    public class CountrySummaryController(ICountrySummaryService countrySummaryService)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetSummariesAsync([FromQuery] SearchRequest request, CancellationToken cancellationToken = default)
        {
            return this.Ok(await countrySummaryService.GetSummariesAsync(request, cancellationToken));
        }

        [HttpGet]
        public async Task<IActionResult> GetSummaryAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
        {
            return this.Ok(await countrySummaryService.GetSummaryAsync(request, cancellationToken));
        }
    }
}
