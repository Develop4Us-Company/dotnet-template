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
    public class StateController(IStateService stateService)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
        {
            return this.Ok(await stateService.GetEntityAsync(request, cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] CreateOrUpdateRequest<State> request, CancellationToken cancellationToken)
        {
            return this.Ok(await stateService.PostEntityAsync(request, cancellationToken));
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] CreateOrUpdateRequest<State> request, CancellationToken cancellationToken)
        {
            return this.Ok(await stateService.PutEntityAsync(request, cancellationToken));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] DeleteRequest<Guid> request, CancellationToken cancellationToken)
        {
            return this.Ok(await stateService.DeleteEntityAsync(request, cancellationToken));
        }
    }
}
