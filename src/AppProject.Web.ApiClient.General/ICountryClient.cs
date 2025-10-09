using System;
using AppProject.Models;
using AppProject.Web.Models.General;
using Refit;

namespace AppProject.Web.ApiClient.General;

public interface ICountryClient
{
    [Get("/api/general/Country/Get")]
    public Task<EntityResponse<Country>> GetAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);

    [Post("/api/general/Country/Post")]
    public Task<KeyResponse<Guid>> PostAsync([Body] CreateOrUpdateRequest<Country> request, CancellationToken cancellationToken = default);

    [Put("/api/general/Country/Put")]
    public Task<KeyResponse<Guid>> PutAsync([Body] CreateOrUpdateRequest<Country> request, CancellationToken cancellationToken = default);

    [Delete("/api/general/Country/Delete")]
    public Task<EmptyResponse> DeleteAsync([Query] DeleteRequest<Guid> request, CancellationToken cancellationToken = default);
}
