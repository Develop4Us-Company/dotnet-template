using System;
using Hangfire;

namespace AppProject.Core.Infrastructure.Jobs;

public class JobDispatcher : IJobDispatcher
{
    public string Enqueue<TJob>()
        where TJob : class, IJob
    {
        return BackgroundJob.Enqueue<TJob>(job => job.ExecuteAsync(CancellationToken.None));
    }

    public string Schedule<TJob>(TimeSpan delay)
        where TJob : class, IJob
    {
        return BackgroundJob.Schedule<TJob>(job => job.ExecuteAsync(CancellationToken.None), delay);
    }

    public string Schedule<TJob>(DateTimeOffset enqueueAt)
        where TJob : class, IJob
    {
        return BackgroundJob.Schedule<TJob>(job => job.ExecuteAsync(CancellationToken.None), enqueueAt);
    }
}
