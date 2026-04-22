using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Courses;

public sealed record CourseStudent
{
    public required CourseId CourseId { get; init; }
    public required StudentId StudentId { get; init; }
}
