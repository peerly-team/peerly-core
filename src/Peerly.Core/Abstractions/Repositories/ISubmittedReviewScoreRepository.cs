using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.Abstractions.Repositories;

public interface ISubmittedReviewScoreRepository : IReadOnlySubmittedReviewScoreRepository
{
    Task BatchAddAsync(IReadOnlyCollection<SubmittedReviewScoreAddItem> items, CancellationToken cancellationToken);
    Task DeleteBySubmittedReviewIdAsync(SubmittedReviewId submittedReviewId, CancellationToken cancellationToken);
}

public interface IReadOnlySubmittedReviewScoreRepository
{
    Task<IReadOnlyCollection<SubmittedReviewScore>> ListBySubmittedReviewIdAsync(
        SubmittedReviewId submittedReviewId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SubmittedReviewScore>> ListBySubmittedReviewIdsAsync(
        IReadOnlyCollection<SubmittedReviewId> submittedReviewIds,
        CancellationToken cancellationToken);
}
