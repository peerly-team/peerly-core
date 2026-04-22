using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetAssignedReview;

internal static class GetAssignedReviewHandlerMapper
{
    public static SubmittedHomeworkStudent ToSubmittedHomeworkStudent(this GetAssignedReviewQuery query)
    {
        return new SubmittedHomeworkStudent
        {
            SubmittedHomeworkId = query.SubmittedHomeworkId,
            StudentId = query.StudentId
        };
    }
}
