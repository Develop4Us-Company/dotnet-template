using System;
using AppProject.Core.Models.General;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public interface IStateService
    : ITransientService,
    IGetEntity<GetByIdRequest<Guid>, EntityResponse<State>>,
    IPostEntity<CreateOrUpdateRequest<State>, KeyResponse<Guid>>,
    IPutEntity<CreateOrUpdateRequest<State>, KeyResponse<Guid>>,
    IDeleteEntity<DeleteRequest<Guid>, EmptyResponse>
{
}
