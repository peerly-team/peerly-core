using System.Collections.Generic;
using Peerly.Core.Models.Groups;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.ListStudentCourseGroups;

internal static class ListStudentCourseGroupsHandlerMapper
{
    public static GroupFilter ToGroupFilter(this ListStudentCourseGroupsQuery query)
    {
        return GroupFilter.Empty() with
        {
            CourseIds = [query.CourseId]
        };
    }

    public static GroupStudentFilter ToGroupStudentFilter(this ListStudentCourseGroupsQuery query, IReadOnlyCollection<Group> groupsOfCourse)
    {
        return new GroupStudentFilter
        {
            StudentIds = [query.StudentId],
            GroupIds = groupsOfCourse.ToArrayBy(group => group.Id)
        };
    }
}
