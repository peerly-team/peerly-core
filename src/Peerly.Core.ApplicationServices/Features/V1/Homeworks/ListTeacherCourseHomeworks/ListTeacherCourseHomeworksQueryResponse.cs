using System.Collections.Generic;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListTeacherCourseHomeworks;

public sealed record ListTeacherCourseHomeworksQueryResponse
{
    public required IReadOnlyCollection<Homework> Homeworks { get; init; }
}
