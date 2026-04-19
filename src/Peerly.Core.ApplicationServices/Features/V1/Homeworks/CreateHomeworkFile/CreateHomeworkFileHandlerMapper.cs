using System;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;

internal static class CreateHomeworkFileHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this CreateHomeworkFileCommand command, CourseId courseId)
    {
        return new CourseTeacher
        {
            CourseId = courseId,
            TeacherId = command.TeacherId
        };
    }

    public static FileAddItem ToFileAddItem(this CreateHomeworkFileCommand command, DateTimeOffset creationTime)
    {
        return new FileAddItem
        {
            StorageId = command.StorageId,
            Name = command.FileName,
            Size = command.FileSize,
            CreationTime = creationTime
        };
    }

    public static HomeworkFileAddItem ToHomeworkFileAddItem(this CreateHomeworkFileCommand command, FileId fileId)
    {
        return new HomeworkFileAddItem
        {
            HomeworkId = command.HomeworkId,
            FileId = fileId,
            TeacherId = command.TeacherId
        };
    }
}
