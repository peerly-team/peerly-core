using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.DeleteGroup;

internal static class DeleteGroupHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this DeleteGroupCommand command, CourseId courseId)
    {
        return new CourseTeacher
        {
            CourseId = courseId,
            TeacherId = command.TeacherId
        };
    }

    public static HomeworkFilter ToHomeworkFilter(this DeleteGroupCommand command)
    {
        return HomeworkFilter.Empty() with
        {
            GroupIds = [command.GroupId]
        };
    }
}
