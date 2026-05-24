using Peerly.Core.Identifiers;
using Peerly.Core.Models.Students;

namespace Peerly.Core.Models.Submissions;

public sealed record SubmittedHomeworkOverview
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required Student Student { get; init; }
    public required int ReviewCount { get; init; }
    public int? ReviewersMark { get; init; }
    public bool? HasDiscrepancy { get; init; }
    public int? TeacherMark { get; init; }
}
