using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListTeacherCourseHomeworks;

public sealed record ListTeacherCourseHomeworksQuery : IQuery<ListTeacherCourseHomeworksQueryResponse>
{
    public required TeacherId TeacherId { get; init; }
    public required CourseId CourseId { get; init; }
}
