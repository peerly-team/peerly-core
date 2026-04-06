using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Submissions;

public sealed record AssignedReview
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required HomeworkId HomeworkId { get; init; }
    public required string HomeworkName { get; init; }
    public required bool IsReviewed { get; init; }
}
