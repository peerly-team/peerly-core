using System;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup;

internal static class CreateGroupHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this CreateGroupCommand command)
    {
        return new CourseTeacher
        {
            CourseId = command.CourseId,
            TeacherId = command.TeacherId
        };
    }

    public static GroupAddItem ToGroupAddItem(this CreateGroupCommand command, DateTimeOffset creationTime)
    {
        return new GroupAddItem
        {
            CourseId = command.CourseId,
            Name = command.Name,
            CreationTime = creationTime
        };
    }
}
