using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetStudentCourse;

internal static class GetStudentCourseHandlerMapper
{
    public static CourseStudent ToCourseStudent(this GetStudentCourseQuery query)
    {
        return new CourseStudent
        {
            CourseId = query.CourseId,
            StudentId = query.StudentId
        };
    }
}
