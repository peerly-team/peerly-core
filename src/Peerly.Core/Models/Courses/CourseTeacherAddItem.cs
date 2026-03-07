using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Courses;

public sealed record CourseTeacherAddItem
{
    public required CourseId CourseId { get; init; }
    public required TeacherId TeacherId { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
