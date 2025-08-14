using System;
using Microsoft.Extensions.Logging;

namespace AppProject.Core.Infrastructure.Jobs;

public abstract class JobBase(ILogger<JobBase> logger)
    : IJob
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting job {JobName} at {Time}.", this.GetType().Name, DateTime.UtcNow);
            await this.RunAsync(cancellationToken);
            logger.LogInformation("Completed job {JobName} at {Time}.", this.GetType().Name, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed job {JobName} at {Time}.", this.GetType().Name, DateTime.UtcNow);
            throw;
        }
    }

    protected abstract Task RunAsync(CancellationToken cancellationToken);
}
