namespace Peerly.Core.Models.Submissions;

public sealed record SubmittedHomeworkMark
{
    public required int ReviewersMark { get; init; }
    public required int? TeacherMark { get; init; }
    public required bool HasDiscrepancy { get; init; }
}
