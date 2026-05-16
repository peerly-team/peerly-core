using System.Collections.Generic;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Homeworks;

public sealed record TeacherHomeworkSearchFilter
{
    public required IReadOnlyCollection<CourseId> CourseIds { get; init; }
    public required IReadOnlyCollection<GroupId> GroupIds { get; init; }
    public required IReadOnlyCollection<HomeworkStatus> HomeworkStatuses { get; init; }
}
