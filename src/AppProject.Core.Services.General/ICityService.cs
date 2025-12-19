using System;
using AppProject.Core.Models.General;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public interface ICityService
    : ITransientService,
    IGetEntity<GetByIdRequest<Guid>, EntityResponse<City>>,
    IPostEntity<CreateOrUpdateRequest<City>, KeyResponse<Guid>>,
    IPutEntity<CreateOrUpdateRequest<City>, KeyResponse<Guid>>,
    IDeleteEntity<DeleteRequest<Guid>, EmptyResponse>
{
    Task<EntitiesResponse<Neighborhood>> GetNeighborhoodEntitiesAsync(GetByParentIdRequest<Guid> request, CancellationToken cancellationToken = default);
}
