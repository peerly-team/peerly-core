using System.Collections.Generic;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile.Abstractions;
using Peerly.Core.ApplicationServices.Services.Anonymization.Models;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Students;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;

internal sealed class CreateSubmittedHomeworkFileHandlerMapper : ICreateSubmittedHomeworkFileHandlerMapper
{
    private readonly IClock _clock;

    public CreateSubmittedHomeworkFileHandlerMapper(IClock clock)
    {
        _clock = clock;
    }

    public FileAddItem ToFileAddItem(CreateSubmittedHomeworkFileCommand command)
    {
        return new FileAddItem
        {
            StorageId = command.StorageId,
            Name = command.FileName,
            Size = command.FileSize,
            CreationTime = _clock.GetCurrentTime()
        };
    }

    public FileAddItem ToAnonymizedFileAddItem(CreateSubmittedHomeworkFileCommand command, AnonymizationResponse response)
    {
        return new FileAddItem
        {
            StorageId = response.AnonymizedStorageId,
            Name = command.FileName,
            Size = response.Size,
            CreationTime = _clock.GetCurrentTime()
        };
    }

    public static AnonymizationRequest ToAnonymizationItem(
        CreateSubmittedHomeworkFileCommand command,
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
