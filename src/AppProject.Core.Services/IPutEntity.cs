using System;
using AppProject.Models;

namespace AppProject.Core.Services;

public interface IPutEntity<TRequest, TResponse>
    where TRequest : class, IRequest
    where TResponse : class, IResponse
{
    Task<TResponse> PutEntityAsync(TRequest request, CancellationToken cancellationToken = default);
}
