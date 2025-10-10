using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.General;
using AppProject.Core.Models.General;
using AppProject.Exceptions;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public class CitySummaryService(
    IDatabaseRepository databaseRepository)
    : BaseService, ICitySummaryService
{
    public async Task<SummariesResponse<CitySummary>> GetSummariesAsync(CitySummarySearchRequest request, CancellationToken cancellationToken = default)
    {
        var searchText = request.SearchText?.Trim();

        var citySummaries = await databaseRepository.GetByConditionAsync<TbCity, CitySummary>(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.Id.ToString().Contains(searchText) || x.Name.Contains(searchText) || (x.Code ?? string.Empty).Contains(searchText));
                }

                if (request.StateId.HasValue)
                {
                    query = query.Where(x => x.StateId == request.StateId.Value);
                }

                query = query.OrderBy(x => x.Name);

                if (request.Take.HasValue)
                {
                    query = query.Take(request.Take.Value);
                }

                return query;
            },
            cancellationToken);

        return new SummariesResponse<CitySummary>
        {
            Summaries = citySummaries
        };
    }

    public async Task<SummaryResponse<CitySummary>> GetSummaryAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        var citySummary = await databaseRepository.GetFirstOrDefaultAsync<TbCity, CitySummary>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (citySummary == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new SummaryResponse<CitySummary>
        {
            Summary = citySummary
        };
    }
}
