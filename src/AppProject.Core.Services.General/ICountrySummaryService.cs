using System;
using AppProject.Core.Models.General;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public interface ICountrySummaryService
    : ITransientService,
    IGetSummaries<SearchRequest, SummariesResponse<CountrySummary>>,
    IGetSummary<GetByIdRequest<Guid>, SummaryResponse<CountrySummary>>
{
}
