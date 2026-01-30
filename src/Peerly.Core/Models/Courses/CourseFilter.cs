using System.Collections.Generic;

namespace Peerly.Core.Models.Courses;

public sealed record CourseFilter
{
    public required IReadOnlyCollection<CourseStatus> CourseStatuses { get; init; }
}
