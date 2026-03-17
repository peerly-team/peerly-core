using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetStudentCourse;

public sealed record GetStudentCourseQuery : IQuery<GetStudentCourseQueryResponse>
{
    public required CourseId CourseId { get; init; }
    public required StudentId StudentId { get; init; }
}
