namespace Peerly.Core.Models.Courses;

public sealed record CourseGroupStudentCount
{
    public required long CourseId { get; init; }
    public required long GroupId { get; init; }
    public required int StudentCount { get; init; }
}
