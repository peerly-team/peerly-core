namespace Peerly.Core.Models.Courses;

public sealed record CourseHomeworkCount
{
    public required long CourseId { get; init; }
    public required int HomeworkCount { get; init; }
}
