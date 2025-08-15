using System;
using AppProject.Core.Models.General;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public interface ICountryService : ITransientService, IGetEntities<GetByIdRequest<Guid>, EntitiesResponse<Country>>
{
}
