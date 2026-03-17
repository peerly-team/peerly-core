using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.UpdateCourse;

internal static class UpdateCourseHandlerMapper
{
    public static CourseTeacherExistsItem ToCourseTeacherExistsItem(this UpdateCourseCommand command)
    {
        return new CourseTeacherExistsItem
        {
            CourseId = command.CourseId,
            TeacherId = command.TeacherId
        };
    }
}
