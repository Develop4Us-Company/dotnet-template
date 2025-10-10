using System;
using AppProject.Core.Models.General;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public interface IStateSummaryService
    : ITransientService,
    IGetSummaries<StateSummarySearchRequest, SummariesResponse<StateSummary>>,
    IGetSummary<GetByIdRequest<Guid>, SummaryResponse<StateSummary>>
{
}
