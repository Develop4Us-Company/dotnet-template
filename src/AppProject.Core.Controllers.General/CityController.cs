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
    public class CityController(ICityService cityService)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
        {
            return this.Ok(await cityService.GetEntityAsync(request, cancellationToken));
        }

        [HttpGet]
        public async Task<IActionResult> GetNeighborhoodsAsync([FromQuery] GetByParentIdRequest<Guid> request, CancellationToken cancellationToken)
        {
            return this.Ok(await cityService.GetNeighborhoodEntitiesAsync(request, cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] CreateOrUpdateRequest<City> request, CancellationToken cancellationToken)
        {
            return this.Ok(await cityService.PostEntityAsync(request, cancellationToken));
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] CreateOrUpdateRequest<City> request, CancellationToken cancellationToken)
        {
            return this.Ok(await cityService.PutEntityAsync(request, cancellationToken));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] DeleteRequest<Guid> request, CancellationToken cancellationToken)
        {
            return this.Ok(await cityService.DeleteEntityAsync(request, cancellationToken));
        }
    }
}
