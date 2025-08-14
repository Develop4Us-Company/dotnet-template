using System;
using AppProject.Core.Infrastructure.Jobs;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace AppProject.Core.Services.General;

[DisableConcurrentExecution(timeoutInSeconds: 60)]
public class SampleJob(ILogger<SampleJob> logger)
    : JobBase(logger)
{
    protected override Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing SampleJob at {Time}.", DateTime.UtcNow);

        return Task.CompletedTask;
    }
}
