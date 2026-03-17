using System.Collections.Generic;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.Shared.SearchCourseHomeworks;

public sealed record SearchCourseHomeworksQueryFilter
{
    public required IReadOnlyCollection<HomeworkStatus> HomeworkStatuses { get; init; }
}
