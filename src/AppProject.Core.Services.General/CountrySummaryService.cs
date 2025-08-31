using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Models.General;
using AppProject.Core.Services.Auth;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public class CountrySummaryService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, ICountrySummaryService
{
    public Task<SummariesResponse<CountrySummary>> GetSummariesAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SummaryResponse<CountrySummary>> GetSummaryAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
