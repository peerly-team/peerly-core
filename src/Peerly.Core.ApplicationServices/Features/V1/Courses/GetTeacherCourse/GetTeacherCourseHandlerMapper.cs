using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetTeacherCourse;

internal static class GetTeacherCourseHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this GetTeacherCourseQuery query)
    {
        return new CourseTeacher
        {
            CourseId = query.CourseId,
            TeacherId = query.TeacherId
        };
    }
}
