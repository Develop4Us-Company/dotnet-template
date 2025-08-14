using System;
using AppProject.Models;

namespace AppProject.Core.Services;

public interface IGetEntity<TRequest, TResponse>
    where TRequest : class, IRequest
    where TResponse : class, IResponse
{
    Task<TResponse> GetEntityAsync(TRequest request, CancellationToken cancellationToken = default);
}
