using System;
using AppProject.Models;
using AppProject.Web.Models.General;
using Refit;

namespace AppProject.Web.ApiClient.General;

public interface ICountrySummaryClient
{
    [Get("/api/general/CountrySummary/GetSummaries")]
    public Task<SummariesResponse<CountrySummary>> GetSummariesAsync([Query] SearchRequest request, CancellationToken cancellationToken = default);

    [Get("/api/general/CountrySummary/GetSummary")]
    public Task<SummaryResponse<CountrySummary>> GetSummaryAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);
}
