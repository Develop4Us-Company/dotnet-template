using System;
using AppProject.Core.Services.General;
using Hangfire;

namespace AppProject.Core.API.Bootstraps;

public static class JobsBootstrap
{
    public static void RegisterRecurringJobs()
    {
        // Register your recurring jobs here
        /*RecurringJob.AddOrUpdate<YourJobName>(
            recurringJobId: nameof(YourJobName),
            job => job.ExecuteAsync(CancellationToken.None),
            cronExpression: Cron.Daily,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            });*/

        RecurringJob.AddOrUpdate<SampleJob>(
            recurringJobId: nameof(SampleJob),
            job => job.ExecuteAsync(CancellationToken.None),
            cronExpression: Cron.Daily,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            });
    }
}
