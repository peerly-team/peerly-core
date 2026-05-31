using System.Collections.Generic;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.Shared.Models;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.Shared;

internal static class SubmittedReviewScoreMapper
{
    public static IReadOnlyCollection<SubmittedReviewScoreAddItem> ToSubmittedReviewScoreAddItems(
        this IReadOnlyCollection<SubmittedReviewScoreItem> scores,
        SubmittedReviewId submittedReviewId)
    {
        return scores.ToArrayBy(
            s => new SubmittedReviewScoreAddItem
            {
                SubmittedReviewId = submittedReviewId,
                RubricCriterionId = s.RubricCriterionId,
                Score = s.Score,
                Comment = s.Comment
            });
    }
}
