using Peerly.Core.Identifiers;
using Peerly.Core.Models.BackgroundService.ReviewCompletions;
using Peerly.Core.Persistence.Repositories.ReviewCompletions.Models;

namespace Peerly.Core.Persistence.Repositories.ReviewCompletions;

internal static class ReviewCompletionRepositoryMapper
{
    public static ReviewCompletionJobItem ToReviewCompletionJobItem(this ReviewCompletionJobItemDb jobItemDb)
    {
        return new ReviewCompletionJobItem
        {
            HomeworkId = new HomeworkId(jobItemDb.HomeworkId),
            CompletionTime = jobItemDb.CompletionTime
        };
    }
}
