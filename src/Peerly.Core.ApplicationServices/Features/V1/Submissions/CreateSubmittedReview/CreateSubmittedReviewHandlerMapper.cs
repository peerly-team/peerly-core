using System;
using System.Linq;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;

internal static class CreateSubmittedReviewHandlerMapper
{
    public static SubmittedReviewAddItem ToSubmittedReviewAddItem(this CreateSubmittedReviewCommand command, DateTimeOffset creationTime)
    {
        return new SubmittedReviewAddItem
        {
            SubmittedHomeworkId = command.SubmittedHomeworkId,
            StudentId = command.StudentId,
            Mark = command.Scores.Sum(s => s.Score),
            Comment = command.Comment,
            CreationTime = creationTime
        };
    }
}
