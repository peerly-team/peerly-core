using System;

namespace Peerly.Core.Persistence.Repositories.SubmittedReviews.Models;

internal sealed record SubmittedReviewDb
{
    public required long Id { get; init; }
    public required long SubmittedHomeworkId { get; init; }
    public required long StudentId { get; init; }
    public required int Mark { get; init; }
    public required string Comment { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
