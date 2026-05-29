using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Persistence.Repositories.SubmittedReviewScores.Models;

namespace Peerly.Core.Persistence.Repositories.SubmittedReviewScores;

internal static class SubmittedReviewScoreRepositoryMapper
{
    public static SubmittedReviewScore ToSubmittedReviewScore(this SubmittedReviewScoreDb db)
    {
        return new SubmittedReviewScore
        {
            Id = new SubmittedReviewScoreId(db.Id),
            SubmittedReviewId = new SubmittedReviewId(db.SubmittedReviewId),
            RubricCriterionId = new RubricCriterionId(db.RubricCriteriaId),
            Score = db.Score,
            Comment = db.Comment
        };
    }
}
