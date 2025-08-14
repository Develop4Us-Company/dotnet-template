using System;
using AppProject.Models;

namespace AppProject.Core.Services;

public interface IPostEntity<TRequest, TResponse>
    where TRequest : class, IRequest
    where TResponse : class, IResponse
{
    Task<TResponse> PostEntityAsync(TRequest request, CancellationToken cancellationToken = default);
}
