using Peerly.Core.ApplicationServices.Executors.Shared.Abstractions;

namespace Peerly.Core.ApplicationServices.BackgroundServices.ReviewCompletion.Options;

internal sealed class ReviewCompletionJobOptions : IMassExecutorOptions
{
    public const string SectionName = "ReviewCompletionJob";

    public int MaxFailCount { get; set; }
    public int ProcessTimeoutSeconds { get; set; }
    public int MaxDegreeOfParallelism { get; set; }
    public int BatchSize { get; set; }
}
