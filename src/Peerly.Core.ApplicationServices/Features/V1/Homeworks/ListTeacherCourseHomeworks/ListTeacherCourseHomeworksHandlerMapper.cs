using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListTeacherCourseHomeworks;

internal static class ListTeacherCourseHomeworksHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this ListTeacherCourseHomeworksQuery query)
    {
        return new CourseTeacher
        {
            CourseId = query.CourseId,
            TeacherId = query.TeacherId
        };
    }
}
