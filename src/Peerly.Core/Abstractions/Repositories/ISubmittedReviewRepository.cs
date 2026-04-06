using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.Abstractions.Repositories;

public interface ISubmittedReviewRepository : IReadOnlySubmittedReviewRepository
{
    Task<SubmittedReviewId> AddAsync(SubmittedReviewAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlySubmittedReviewRepository
{
    Task<bool> ExistsAsync(SubmittedHomeworkStudent submittedHomeworkStudent, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SubmittedHomeworkReviewerMark>> ListSubmittedReviewMarksAsync(
        HomeworkId homeworkId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SubmittedReview>> ListBySubmittedHomeworkIdAsync(
        SubmittedHomeworkId submittedHomeworkId,
        CancellationToken cancellationToken);
}
