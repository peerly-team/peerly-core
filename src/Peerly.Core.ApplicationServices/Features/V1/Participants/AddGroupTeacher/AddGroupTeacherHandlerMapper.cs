using System;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Teachers;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupTeacher;

internal static class AddGroupTeacherHandlerMapper
{
    public static GroupFilter ToGroupFilter(this AddGroupTeacherCommand command)
    {
        return GroupFilter.Empty() with
        {
            GroupIds = [command.GroupId]
        };
    }

    public static TeacherFilter ToTeacherFilter(this AddGroupTeacherCommand command)
    {
        var teacherIds = command.TeacherId == command.ActorTeacherId
            ? new[] { command.TeacherId }
            : new[] { command.TeacherId, command.ActorTeacherId };

        return new TeacherFilter
        {
            TeacherIds = teacherIds
        };
    }

    public static CourseTeacherExistsItem ToCourseTeacherExistsItem(this AddGroupTeacherCommand command, CourseId courseId)
    {
        return new CourseTeacherExistsItem
        {
            CourseId = courseId,
            TeacherId = command.ActorTeacherId
        };
    }

    public static GroupTeacherFilter ToGroupTeacherFilter(this AddGroupTeacherCommand command)
    {
        return new GroupTeacherFilter
        {
            GroupIds = [command.GroupId],
            TeacherIds = [command.TeacherId]
        };
    }

    public static GroupTeacherAddItem ToGroupTeacherAddItem(this AddGroupTeacherCommand command, DateTimeOffset currentTime)
    {
        return new GroupTeacherAddItem
        {
            GroupId = command.GroupId,
            TeacherId = command.TeacherId,
            CreationTime = currentTime
        };
    }
}
