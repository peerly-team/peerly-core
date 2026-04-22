namespace Peerly.Core.Models.Submissions;

public sealed record SubmittedReviewUpdateItem
{
    public required int Mark { get; init; }
    public required string Comment { get; init; }
}
