using System;
using AppProject.Models;
using AppProject.Web.Models.General;
using Refit;

namespace AppProject.Web.ApiClient.General;

public interface ICityClient
{
    [Get("/api/general/City/Get")]
    public Task<EntityResponse<City>> GetAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);

    [Get("/api/general/City/GetNeighborhoods")]
    public Task<EntitiesResponse<Neighborhood>> GetNeighborhoodsAsync([Query] GetByParentIdRequest<Guid> request, CancellationToken cancellationToken = default);

    [Post("/api/general/City/Post")]
    public Task<KeyResponse<Guid>> PostAsync([Body] CreateOrUpdateRequest<City> request, CancellationToken cancellationToken = default);

    [Put("/api/general/City/Put")]
    public Task<KeyResponse<Guid>> PutAsync([Body] CreateOrUpdateRequest<City> request, CancellationToken cancellationToken = default);

    [Delete("/api/general/City/Delete")]
    public Task<EmptyResponse> DeleteAsync([Query] DeleteRequest<Guid> request, CancellationToken cancellationToken = default);
}
