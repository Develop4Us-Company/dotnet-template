using System;
using AppProject.Models;
using AppProject.Web.Models.General;
using Refit;

namespace AppProject.Web.ApiClient.General;

public interface IStateSummaryClient
{
    [Get("/api/general/StateSummary/GetSummaries")]
    public Task<SummariesResponse<StateSummary>> GetSummariesAsync([Query] StateSummarySearchRequest request, CancellationToken cancellationToken = default);

    [Get("/api/general/StateSummary/GetSummary")]
    public Task<SummaryResponse<StateSummary>> GetSummaryAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);
}
