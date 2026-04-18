using System.Collections.Generic;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Teachers;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.ListCourseParticipants;

internal static class ListCourseParticipantsHandlerMapper
{
    public static GroupFilter ToGroupFilter(this ListCourseParticipantsQuery query)
    {
        return GroupFilter.Empty() with
        {
            CourseIds = [query.CourseId]
        };
    }

    public static GroupStudentFilter ToGroupStudentFilter(IReadOnlyCollection<GroupId> groupIds)
    {
        return GroupStudentFilter.Empty() with
        {
            GroupIds = groupIds
        };
    }

    public static StudentFilter ToStudentFilter(IReadOnlyCollection<StudentId> studentIds)
    {
        return new StudentFilter
        {
            StudentIds = studentIds
        };
    }

    public static TeacherFilter ToTeacherFilter(IReadOnlyCollection<TeacherId> teacherIds)
    {
        return new TeacherFilter
        {
            TeacherIds = teacherIds
        };
    }
}
