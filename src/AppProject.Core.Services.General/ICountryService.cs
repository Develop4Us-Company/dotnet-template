using System;
using AppProject.Core.Models.General;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public interface ICountryService
    : ITransientService,
    IGetEntity<GetByIdRequest<Guid>, EntityResponse<Country>>,
    IPostEntity<CreateOrUpdateRequest<Country>, KeyResponse<Guid>>,
    IPutEntity<CreateOrUpdateRequest<Country>, KeyResponse<Guid>>,
    IDeleteEntity<DeleteRequest<Guid>, EmptyResponse>
{
}
