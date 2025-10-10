using System;
using AppProject.Models;
using AppProject.Web.Models.General;
using Refit;

namespace AppProject.Web.ApiClient.General;

public interface ICitySummaryClient
{
    [Get("/api/general/CitySummary/GetSummaries")]
    public Task<SummariesResponse<CitySummary>> GetSummariesAsync([Query] CitySummarySearchRequest request, CancellationToken cancellationToken = default);

    [Get("/api/general/CitySummary/GetSummary")]
    public Task<SummaryResponse<CitySummary>> GetSummaryAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);
}
