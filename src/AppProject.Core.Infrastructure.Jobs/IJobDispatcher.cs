using System;

namespace AppProject.Core.Infrastructure.Jobs;

public interface IJobDispatcher
{
    string Enqueue<TJob>()
        where TJob : class, IJob;

    string Schedule<TJob>(TimeSpan delay)
        where TJob : class, IJob;

    string Schedule<TJob>(DateTimeOffset enqueueAt)
        where TJob : class, IJob;
}
