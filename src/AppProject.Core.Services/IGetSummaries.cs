using System;
using AppProject.Models;

namespace AppProject.Core.Services;

public interface IGetSummaries<TRequest, TResponse>
    where TRequest : class, IRequest
    where TResponse : class, IResponse
{
    Task<TResponse> GetSummariesAsync(TRequest request, CancellationToken cancellationToken = default);
}
