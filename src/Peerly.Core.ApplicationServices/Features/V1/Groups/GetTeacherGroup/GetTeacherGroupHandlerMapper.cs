using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.GetTeacherGroup;

internal static class GetTeacherGroupHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this GetTeacherGroupQuery query, CourseId courseId)
    {
        return new CourseTeacher
        {
            CourseId = courseId,
            TeacherId = query.TeacherId
        };
    }

    public static GroupTeacher ToGroupTeacher(this GetTeacherGroupQuery query)
    {
        return new GroupTeacher
        {
            GroupId = query.GroupId,
            TeacherId = query.TeacherId
        };
    }
}
