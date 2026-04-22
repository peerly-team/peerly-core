using System.Collections.Generic;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Teachers;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.ListGroupParticipants;

internal static class ListGroupParticipantsHandlerMapper
{
    public static GroupStudentFilter ToGroupStudentFilter(GroupId groupId)
    {
        return GroupStudentFilter.Empty() with
        {
            GroupIds = [groupId]
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
