using System.Collections.Generic;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchTeacherCourseHomeworks;

public sealed record SearchTeacherCourseHomeworksQueryResponse
{
    public required IReadOnlyCollection<Homework> Homeworks { get; init; }
}
