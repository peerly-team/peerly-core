using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetTeacherCourse;

internal static class GetTeacherCourseHandlerMapper
{
    public static CourseTeacherExistsItem ToCourseTeacherExistsItem(this GetTeacherCourseQuery query)
    {
        return new CourseTeacherExistsItem
        {
            CourseId = query.CourseId,
            TeacherId = query.TeacherId
        };
    }
}
