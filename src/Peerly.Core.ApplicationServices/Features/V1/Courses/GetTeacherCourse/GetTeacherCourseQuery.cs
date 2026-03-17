using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetTeacherCourse;

public sealed record GetTeacherCourseQuery : IQuery<GetTeacherCourseQueryResponse>
{
    public required CourseId CourseId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
