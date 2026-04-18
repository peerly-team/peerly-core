namespace Peerly.Core.Persistence.Repositories.SubmittedHomeworkMarks.Models;

internal sealed record SubmittedHomeworkMarkDb
{
    public required long SubmittedHomeworkId { get; init; }
    public required int ReviewersMark { get; init; }
    public required int? TeacherMark { get; init; }
    public required long? TeacherId { get; init; }
    public required bool HasDiscrepancy { get; init; }
}
