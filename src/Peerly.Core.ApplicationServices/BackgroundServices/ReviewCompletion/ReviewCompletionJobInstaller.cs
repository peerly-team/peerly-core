using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions.Executors;
using Peerly.Core.ApplicationServices.BackgroundServices.ReviewCompletion.Options;
using Peerly.Core.ApplicationServices.Executors.Shared;
using Peerly.Core.Models.BackgroundService.ReviewCompletions;
using Peerly.Core.Tools.Abstractions;
using Quartz;

namespace Peerly.Core.ApplicationServices.BackgroundServices.ReviewCompletion;

[ExcludeFromCodeCoverage]
internal sealed class ReviewCompletionJobInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddOptions<ReviewCompletionJobOptions>()
            .BindConfiguration(ReviewCompletionJobOptions.SectionName);

        services
            .AddScoped<IMassExecutor<ReviewCompletionJobItem>,
                ConcurrentMassExecutorAdapter<ReviewCompletionJobItem, ReviewCompletionJobOptions>>()
            .AddScoped<IExecutor<ReviewCompletionJobItem>, ReviewCompletionJobExecutor>();

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey(nameof(ReviewCompletionJob));

            q.AddJob<ReviewCompletionJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{nameof(ReviewCompletionJob)}-trigger")
                .WithCronSchedule("0 * * * * ?"));
        });

        services.AddQuartzHostedService(opts =>
        {
            opts.WaitForJobsToComplete = true;
        });
    }
}
