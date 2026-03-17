using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetStudentCourse;

public sealed record GetStudentCourseQueryResponse
{
    public required CourseQueryResponseItem CourseInfo { get; init; }
}
