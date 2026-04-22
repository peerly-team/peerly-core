using System;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;

internal static class CreateSubmittedHomeworkHandlerMapper
{
    public static SubmittedHomeworkAddItem ToSubmittedHomeworkAddItem(
        this CreateSubmittedHomeworkCommand command,
        DateTimeOffset creationTime)
    {
        return new SubmittedHomeworkAddItem
        {
            HomeworkId = command.HomeworkId,
            StudentId = command.StudentId,
            Comment = command.Comment,
            CreationTime = creationTime
        };
    }
}
