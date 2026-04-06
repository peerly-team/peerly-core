namespace Peerly.Core.Persistence.Repositories.DistributionReviewers;

internal sealed record AssignedReviewDb
{
    public required long SubmittedHomeworkId { get; init; }
    public required long HomeworkId { get; init; }
    public required string HomeworkName { get; init; }
    public required bool IsReviewed { get; init; }
}
