namespace Peerly.Core.Models.Submissions;

public sealed record SubmittedHomeworkMarkUpdateItem
{
    public required int? TeacherMark { get; init; }
    public required long? TeacherId { get; init; }
}
