using System;

namespace AppProject.Core.Infrastructure.Jobs;

public interface IJob
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
