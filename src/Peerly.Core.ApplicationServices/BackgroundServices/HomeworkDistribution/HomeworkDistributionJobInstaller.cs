using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions.Executors;
using Peerly.Core.ApplicationServices.BackgroundServices.HomeworkDistribution.Abstractions;
using Peerly.Core.ApplicationServices.BackgroundServices.HomeworkDistribution.Options;
using Peerly.Core.ApplicationServices.Executors.Shared;
using Peerly.Core.Models.BackgroundService.HomeworkDistributions;
using Peerly.Core.Tools.Abstractions;
using Quartz;

namespace Peerly.Core.ApplicationServices.BackgroundServices.HomeworkDistribution;

[ExcludeFromCodeCoverage]
internal sealed class HomeworkDistributionJobInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddOptions<HomeworkDistributionJobOptions>()
            .BindConfiguration(HomeworkDistributionJobOptions.SectionName);

        services
            .AddScoped<IMassExecutor<HomeworkDistributionJobItem>,
                ConcurrentMassExecutorAdapter<HomeworkDistributionJobItem, HomeworkDistributionJobOptions>>()
            .AddScoped<IHomeworkDistributionJobValidator, HomeworkDistributionJobValidator>()
            .AddScoped<IExecutor<HomeworkDistributionJobItem>, HomeworkDistributionJobExecutor>();

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey(nameof(HomeworkDistributionJob));

            q.AddJob<HomeworkDistributionJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{nameof(HomeworkDistributionJob)}-trigger")
                .WithCronSchedule("0 * * * * ?"));
        });

        services.AddQuartzHostedService(opts =>
        {
            opts.WaitForJobsToComplete = true;
        });
    }
}
