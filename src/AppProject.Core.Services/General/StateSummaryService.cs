using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.General;
using AppProject.Core.Models.General;
using AppProject.Exceptions;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public class StateSummaryService(
    IDatabaseRepository databaseRepository)
    : BaseService, IStateSummaryService
{
    public async Task<SummariesResponse<StateSummary>> GetSummariesAsync(StateSummarySearchRequest request, CancellationToken cancellationToken = default)
    {
        var searchText = request.SearchText?.Trim();

        var stateSummaries = await databaseRepository.GetByConditionAsync<TbState, StateSummary>(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.Id.ToString().Contains(searchText) || x.Name.Contains(searchText) || (x.Code ?? string.Empty).Contains(searchText));
                }

                if (request.CountryId.HasValue)
                {
                    query = query.Where(x => x.CountryId == request.CountryId.Value);
                }

                query = query.OrderBy(x => x.Name);

                if (request.Take.HasValue)
                {
                    query = query.Take(request.Take.Value);
                }

                return query;
            },
            cancellationToken);

        return new SummariesResponse<StateSummary>
        {
            Summaries = stateSummaries
        };
    }

    public async Task<SummaryResponse<StateSummary>> GetSummaryAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        var stateSummary = await databaseRepository.GetFirstOrDefaultAsync<TbState, StateSummary>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (stateSummary == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new SummaryResponse<StateSummary>
        {
            Summary = stateSummary
        };
    }
}
