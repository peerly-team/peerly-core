using System;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Files;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourseFile;

internal static class CreateCourseFileHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this CreateCourseFileCommand command)
    {
        return new CourseTeacher
        {
            CourseId = command.CourseId,
            TeacherId = command.TeacherId
        };
    }

    public static FileAddItem ToFileAddItem(this CreateCourseFileCommand command, DateTimeOffset creationTime)
    {
        return new FileAddItem
        {
            StorageId = command.StorageId,
            Name = command.FileName,
            Size = command.FileSize,
            CreationTime = creationTime
        };
    }

    public static CourseFileAddItem ToCourseFileAddItem(this CreateCourseFileCommand command, FileId fileId)
    {
        return new CourseFileAddItem
        {
            CourseId = command.CourseId,
            FileId = fileId,
            TeacherId = command.TeacherId
        };
    }
}
