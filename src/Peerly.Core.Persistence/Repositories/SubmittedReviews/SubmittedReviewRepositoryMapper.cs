using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Persistence.Repositories.SubmittedReviews.Models;

namespace Peerly.Core.Persistence.Repositories.SubmittedReviews;

internal static class SubmittedReviewRepositoryMapper
{
    public static SubmittedHomeworkReviewerMark ToSubmittedHomeworkReviewerMark(this SubmittedHomeworkReviewerMarkDb db)
    {
        return new SubmittedHomeworkReviewerMark
        {
            SubmittedHomeworkId = new SubmittedHomeworkId(db.SubmittedHomeworkId),
            ReviewerMark = db.Mark
        };
    }

    public static SubmittedReview ToSubmittedReview(this SubmittedReviewDb db)
    {
        return new SubmittedReview
        {
            Id = new SubmittedReviewId(db.Id),
            StudentId = new StudentId(db.StudentId),
            Mark = db.Mark,
            Comment = db.Comment
        };
    }
}
