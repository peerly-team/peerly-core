using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Courses;

public sealed record CourseTeacherExistsItem
{
    public required CourseId CourseId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
