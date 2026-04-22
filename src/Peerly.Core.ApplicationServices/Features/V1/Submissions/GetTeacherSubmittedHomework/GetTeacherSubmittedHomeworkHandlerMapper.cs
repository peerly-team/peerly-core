using Peerly.Core.Models.Students;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmittedHomework;

internal static class GetTeacherSubmittedHomeworkHandlerMapper
{
    public static TeacherSubmittedReview ToTeacherSubmittedReview(this SubmittedReview review, Student reviewer)
    {
        return new TeacherSubmittedReview
        {
            Review = review,
            Reviewer = reviewer
        };
    }
}
