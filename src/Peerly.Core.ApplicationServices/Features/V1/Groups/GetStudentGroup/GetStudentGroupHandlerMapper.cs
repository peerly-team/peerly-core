using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.GetStudentGroup;

internal static class GetStudentGroupHandlerMapper
{
    public static GroupStudent ToGroupStudent(this GetStudentGroupQuery query)
    {
        return new GroupStudent
        {
            GroupId = query.GroupId,
            StudentId = query.StudentId
        };
    }
}
