using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListStudentCourseHomeworks;

public sealed record ListStudentCourseHomeworksQuery : IQuery<ListStudentCourseHomeworksQueryResponse>
{
    public required StudentId StudentId { get; init; }
    public required CourseId CourseId { get; init; }
}
