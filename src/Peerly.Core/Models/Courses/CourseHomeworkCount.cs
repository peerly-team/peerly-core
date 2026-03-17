using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Courses;

public sealed record CourseHomeworkCount
{
    public required CourseId CourseId { get; init; }
    public required int HomeworkCount { get; init; }
}
