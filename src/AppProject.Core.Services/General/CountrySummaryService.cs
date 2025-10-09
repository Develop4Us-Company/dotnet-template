using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.General;
using AppProject.Core.Models.General;
using AppProject.Exceptions;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public class CountrySummaryService(
    IDatabaseRepository databaseRepository)
    : BaseService, ICountrySummaryService
{
    public async Task<SummariesResponse<CountrySummary>> GetSummariesAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        var searchText = request.SearchText?.Trim();

        var countrySummaries = await databaseRepository.GetByConditionAsync<TbCountry, CountrySummary>(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.Id.ToString().Contains(searchText) || x.Name.Contains(searchText) || (x.Code ?? string.Empty).Contains(searchText));
                }

                query = query.OrderBy(x => x.Name);

                if (request.Take.HasValue)
                {
                    query = query.Take(request.Take.Value);
                }

                return query;
            },
            cancellationToken);

        return new SummariesResponse<CountrySummary>
        {
            Summaries = countrySummaries
        };
    }

    public async Task<SummaryResponse<CountrySummary>> GetSummaryAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        var countrySummary = await databaseRepository.GetFirstOrDefaultAsync<TbCountry, CountrySummary>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (countrySummary == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new SummaryResponse<CountrySummary>
        {
            Summary = countrySummary
        };
    }
}
