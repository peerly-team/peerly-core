using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.UpdateGroup;

internal static class UpdateGroupHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this UpdateGroupCommand command, CourseId courseId)
    {
        return new CourseTeacher
        {
            CourseId = courseId,
            TeacherId = command.TeacherId
        };
    }
}
