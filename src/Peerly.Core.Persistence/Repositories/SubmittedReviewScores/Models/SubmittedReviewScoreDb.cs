namespace Peerly.Core.Persistence.Repositories.SubmittedReviewScores.Models;

internal sealed record SubmittedReviewScoreDb
{
    public required long Id { get; init; }
    public required long SubmittedReviewId { get; init; }
    public required long RubricCriteriaId { get; init; }
    public required int Score { get; init; }
    public string? Comment { get; init; }
}
