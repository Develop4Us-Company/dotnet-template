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
public class CountryController(ICountryService countryService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
    {
        return this.Ok(await countryService.GetEntityAsync(request, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CreateOrUpdateRequest<Country> request, CancellationToken cancellationToken)
    {
        return this.Ok(await countryService.PostEntityAsync(request, cancellationToken));
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync([FromBody] CreateOrUpdateRequest<Country> request, CancellationToken cancellationToken)
    {
        return this.Ok(await countryService.PutEntityAsync(request, cancellationToken));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAsync([FromQuery] DeleteRequest<Guid> request, CancellationToken cancellationToken)
    {
        return this.Ok(await countryService.DeleteEntityAsync(request, cancellationToken));
    }
}
