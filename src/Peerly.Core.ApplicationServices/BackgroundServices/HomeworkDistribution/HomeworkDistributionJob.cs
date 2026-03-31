using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions.Executors;
using Peerly.Core.ApplicationServices.BackgroundServices.HomeworkDistribution.Options;
using Peerly.Core.Models.BackgroundService;
using Peerly.Core.Models.BackgroundService.HomeworkDistributions;
using Quartz;

namespace Peerly.Core.ApplicationServices.BackgroundServices.HomeworkDistribution;

[DisallowConcurrentExecution]
internal sealed class HomeworkDistributionJob : IJob
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IMassExecutor<HomeworkDistributionJobItem> _executor;
    private readonly ILogger<HomeworkDistributionJob> _logger;
    private readonly HomeworkDistributionJobOptions _options;

    public HomeworkDistributionJob(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IMassExecutor<HomeworkDistributionJobItem> executor,
        ILogger<HomeworkDistributionJob> logger,
        IOptions<HomeworkDistributionJobOptions> options)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _executor = executor;
        _logger = logger;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(context.CancellationToken);

            var filter = GetHomeworkDistributionFilter();
            var jobItems = await unitOfWork.HomeworkDistributionRepository.TakeAsync(filter, context.CancellationToken);

            await _executor.RunAsync(jobItems, context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "{Job} | An unexpected error occurred | Error message: {ErrorMessage}",
                nameof(HomeworkDistributionJob),
                ex.Message);
        }
    }

    private HomeworkDistributionFilter GetHomeworkDistributionFilter()
    {
        return new HomeworkDistributionFilter
        {
            ProcessStatuses = [ProcessStatus.Created, ProcessStatus.InProgress, ProcessStatus.Failed],
            MaxFailCount = _options.MaxFailCount,
            ProcessTimeoutSeconds = TimeSpan.FromSeconds(_options.ProcessTimeoutSeconds),
            Limit = _options.BatchSize
        };
    }
}
