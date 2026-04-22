using Peerly.Core.Models.Students;

namespace Peerly.Core.Models.Submissions;

public sealed record TeacherSubmittedReview
{
    public required SubmittedReview Review { get; init; }
    public required Student Reviewer { get; init; }
}
