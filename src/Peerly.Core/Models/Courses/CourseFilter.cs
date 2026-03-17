using System.Collections.Generic;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Courses;

public sealed record CourseFilter
{
    public required IReadOnlyCollection<CourseId> CourseIds { get; init; }
    public required IReadOnlyCollection<CourseStatus> CourseStatuses { get; init; }

    public static CourseFilter Empty()
    {
        return new CourseFilter
        {
            CourseIds = [],
            CourseStatuses = []
        };
    }
}
