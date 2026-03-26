using Peerly.Core.ApplicationServices.Executors.Shared.Abstractions;

namespace Peerly.Core.ApplicationServices.BackgroundServices.HomeworkDistribution;

internal sealed class HomeworkDistributionJobOptions : IMassExecutorOptions
{
    public const string SectionName = "HomeworkDistributionJob";

    public int MaxFailCount { get; set; }
    public int ProcessTimeoutSeconds { get; set; }
    public int MaxDegreeOfParallelism { get; set; }
}
