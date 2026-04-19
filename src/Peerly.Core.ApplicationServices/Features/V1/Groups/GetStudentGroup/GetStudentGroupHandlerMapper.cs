using System.Collections.Generic;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.GetStudentGroup;

internal static class GetStudentGroupHandlerMapper
{
    public static GroupFilter ToGroupFilter(this GetStudentGroupQuery query, CourseId courseId)
    {
        return GroupFilter.Empty() with
        {
            CourseIds = [courseId]
        };
    }

    public static GroupStudentFilter ToGroupStudentFilter(this GetStudentGroupQuery query, IReadOnlyCollection<Group> groupsOfCourse)
    {
        return new GroupStudentFilter
        {
            StudentIds = [query.StudentId],
            GroupIds = groupsOfCourse.ToArrayBy(group => group.Id)
        };
    }
}
