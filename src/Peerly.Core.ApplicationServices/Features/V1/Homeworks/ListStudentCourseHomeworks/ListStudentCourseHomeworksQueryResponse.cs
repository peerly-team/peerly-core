using System.Collections.Generic;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListStudentCourseHomeworks;

public sealed record ListStudentCourseHomeworksQueryResponse
{
    public required IReadOnlyCollection<Homework> Homeworks { get; init; }
}
