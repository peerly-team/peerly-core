using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Peerly.Core.ApplicationServices.Abstractions.Executors;
using Peerly.Core.Models.BackgroundService.HomeworkDistributions;

namespace Peerly.Core.ApplicationServices.BackgroundServices.HomeworkDistribution;

internal sealed class HomeworkDistributionJobExecutor : IExecutor<HomeworkDistributionJobItem>
{
    private readonly ILogger<HomeworkDistributionJobExecutor> _logger;

    public HomeworkDistributionJobExecutor(ILogger<HomeworkDistributionJobExecutor> logger)
    {
        _logger = logger;
    }

    public Task RunAsync(HomeworkDistributionJobItem requestItem, CancellationToken cancellationToken)
    {
        _logger.LogCritical("Шутка! Все хорошо :)");

        return Task.CompletedTask;
    }
}
