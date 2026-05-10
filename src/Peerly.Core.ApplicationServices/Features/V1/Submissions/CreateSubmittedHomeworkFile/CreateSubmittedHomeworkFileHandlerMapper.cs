using System;
using System.Collections.Generic;
using Peerly.Core.ApplicationServices.Services.Anonymization.Models;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;

internal static class CreateSubmittedHomeworkFileHandlerMapper
{
    public static FileAddItem ToFileAddItem(this CreateSubmittedHomeworkFileCommand command, DateTimeOffset creationTime)
    {
        return new FileAddItem
        {
            StorageId = command.StorageId,
            Name = command.FileName,
            Size = command.FileSize,
            CreationTime = creationTime
        };
    }

    public static FileAddItem ToAnonymizedFileAddItem(
        this AnonymizationResult result,
        DateTimeOffset creationTime)
    {
        return new FileAddItem
        {
            StorageId = result.AnonymizedStorageId,
            Name = result.AnonymizedFileName,
            Size = result.Size,
            CreationTime = creationTime
        };
    }

    public static SubmittedHomeworkFileAddItem ToSubmittedHomeworkFileAddItem(
        this CreateSubmittedHomeworkFileCommand command,
        FileId fileId,
        FileId? anonymizedFileId)
    {
        return new SubmittedHomeworkFileAddItem
        {
            SubmittedHomeworkId = command.SubmittedHomeworkId,
            FileId = fileId,
            AnonymizedFileId = anonymizedFileId
        };
    }

    public static AnonymizationRequest ToAnonymizationItem(
        this CreateSubmittedHomeworkFileCommand command,
        IReadOnlyCollection<Student> students)
    {
        return new AnonymizationRequest
        {
            OriginalStorageId = command.StorageId,
            FileName = command.FileName,
            Students = students
        };
    }

    public static GroupStudentFilter ToGroupStudentFilter(GroupId groupId)
    {
        return new GroupStudentFilter
        {
            GroupIds = [groupId],
            StudentIds = []
        };
    }

    public static GroupStudentFilter ToGroupStudentFilter(IReadOnlyCollection<Group> groups)
    {
        return new GroupStudentFilter
        {
            GroupIds = groups.ToArrayBy(g => g.Id),
            StudentIds = []
        };
    }

    public static GroupFilter ToGroupFilter(CourseId courseId)
    {
        return new GroupFilter
        {
            CourseIds = [courseId],
            GroupIds = []
        };
    }

    public static StudentFilter ToStudentFilter(IReadOnlyCollection<GroupStudent> groupStudents)
    {
        return new StudentFilter
        {
            StudentIds = groupStudents.ToArrayBy(gs => gs.StudentId)
        };
    }
}
