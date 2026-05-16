using System.Collections.Generic;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchStudentHomeworks;

public sealed record SearchStudentHomeworksQueryFilter
{
    public required IReadOnlyCollection<HomeworkStatus> HomeworkStatuses { get; init; }
}
