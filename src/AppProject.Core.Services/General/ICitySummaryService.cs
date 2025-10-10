using System;
using AppProject.Core.Models.General;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public interface ICitySummaryService
    : ITransientService,
    IGetSummaries<CitySummarySearchRequest, SummariesResponse<CitySummary>>,
    IGetSummary<GetByIdRequest<Guid>, SummaryResponse<CitySummary>>
{
}
