using System;
using AppProject.Models;

namespace AppProject.Core.Services;

public interface IDeleteEntity<TRequest, TResponse>
    where TRequest : class, IRequest
    where TResponse : class, IResponse
{
    Task<TResponse> DeleteEntityAsync(TRequest request, CancellationToken cancellationToken = default);
}
