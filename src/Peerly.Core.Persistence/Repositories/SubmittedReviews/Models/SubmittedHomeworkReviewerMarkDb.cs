namespace Peerly.Core.Persistence.Repositories.SubmittedReviews.Models;

internal sealed record SubmittedHomeworkReviewerMarkDb
{
    public required long SubmittedHomeworkId { get; init; }
    public required int Mark { get; init; }
}
