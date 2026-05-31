using System.Collections.Generic;
using System.Linq;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.Shared.Models;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Rubrics;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.Shared.Validators;

internal static class SubmittedReviewCommonValidator
{
    internal static CommandValidationResult? ValidateScoresAgainstCriteria(
        IReadOnlyCollection<SubmittedReviewScoreItem> scores,
        IReadOnlyCollection<RubricCriterion> criteria)
    {
        var criteriaById = criteria.ToDictionary(c => c.Id);
        var scoreCriterionIds = scores.Select(s => s.RubricCriterionId).ToHashSet();

        if (scoreCriterionIds.Count != criteria.Count || !criteriaById.Keys.All(scoreCriterionIds.Contains))
        {
            return ValidationError.From(SubmittedReviewErrors.ScoresMismatchCriteria);
        }

        foreach (var score in scores)
        {
            var criterion = criteriaById[score.RubricCriterionId];

            if (score.Score < 0 || score.Score > criterion.MaxScore)
            {
                return ValidationError.From(SubmittedReviewErrors.ScoreOutOfRange);
            }

            if (criterion.CommentRequired && string.IsNullOrWhiteSpace(score.Comment))
            {
                return ValidationError.From(SubmittedReviewErrors.CriterionCommentRequired);
            }
        }

        return null;
    }
}
