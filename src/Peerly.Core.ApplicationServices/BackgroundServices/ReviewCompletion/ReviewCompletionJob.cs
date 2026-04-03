using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions.Executors;
using Peerly.Core.ApplicationServices.BackgroundServices.ReviewCompletion.Options;
using Peerly.Core.Models.BackgroundService;
using Peerly.Core.Models.BackgroundService.ReviewCompletions;
using Quartz;

namespace Peerly.Core.ApplicationServices.BackgroundServices.ReviewCompletion;

[DisallowConcurrentExecution]
internal sealed class ReviewCompletionJob : IJob
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IMassExecutor<ReviewCompletionJobItem> _executor;
    private readonly ILogger<ReviewCompletionJob> _logger;
    private readonly ReviewCompletionJobOptions _options;

    public ReviewCompletionJob(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IMassExecutor<ReviewCompletionJobItem> executor,
        ILogger<ReviewCompletionJob> logger,
        IOptions<ReviewCompletionJobOptions> options)
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

            var filter = GetReviewCompletionFilter();
            var jobItems = await unitOfWork.ReviewCompletionRepository.TakeAsync(filter, context.CancellationToken);

            await _executor.RunAsync(jobItems, context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "{Job} | An unexpected error occurred | Error message: {ErrorMessage}",
                nameof(ReviewCompletionJob),
                ex.Message);
        }
    }

    private ReviewCompletionFilter GetReviewCompletionFilter()
    {
        return new ReviewCompletionFilter
        {
            ProcessStatuses = [ProcessStatus.Created, ProcessStatus.InProgress, ProcessStatus.Failed],
            MaxFailCount = _options.MaxFailCount,
            ProcessTimeoutSeconds = TimeSpan.FromSeconds(_options.ProcessTimeoutSeconds),
            Limit = _options.BatchSize
        };
    }
}
