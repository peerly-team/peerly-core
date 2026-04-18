using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.GetTeacherGroup;

internal static class GetTeacherGroupHandlerMapper
{
    public static GroupTeacher ToGroupTeacher(this GetTeacherGroupQuery query)
    {
        return new GroupTeacher
        {
            GroupId = query.GroupId,
            TeacherId = query.TeacherId
        };
    }
}
