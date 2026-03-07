using System.Collections.Generic;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Homeworks;

public sealed record HomeworkFilter
{
    public required IReadOnlyCollection<CourseId> CourseIds { get; init; }
    public required IReadOnlyCollection<HomeworkStatus> HomeworkStatuses { get; init; }

    public static HomeworkFilter Empty()
    {
        return new HomeworkFilter
        {
            CourseIds = [],
            HomeworkStatuses = []
        };
    }
}
