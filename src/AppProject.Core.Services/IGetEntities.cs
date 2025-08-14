using System;
using AppProject.Models;

namespace AppProject.Core.Services;

public interface IGetEntities<TRequest, TResponse>
    where TRequest : class, IRequest
    where TResponse : class, IResponse
{
    Task<TResponse> GetEntitiesAsync(TRequest request, CancellationToken cancellationToken = default);
}
