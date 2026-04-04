using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Submissions;

public sealed record SubmittedHomeworkReviewerMark
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required int ReviewerMark { get; init; }
}
