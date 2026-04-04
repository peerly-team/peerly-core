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
}
