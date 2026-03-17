using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetTeacherCourse;

public sealed record GetTeacherCourseQueryResponse
{
    public required CourseQueryResponseItem CourseInfo { get; init; }
}
