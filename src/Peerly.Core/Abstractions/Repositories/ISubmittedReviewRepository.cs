using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.Abstractions.Repositories;

public interface ISubmittedReviewRepository : IReadOnlySubmittedReviewRepository
{
    Task<SubmittedReviewId> AddAsync(SubmittedReviewAddItem item, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        SubmittedReviewId submittedReviewId,
        Action<IUpdateBuilder<SubmittedReviewUpdateItem>> configureUpdate,
        CancellationToken cancellationToken);
}

public interface IReadOnlySubmittedReviewRepository
{
    Task<SubmittedReviewId?> GetSubmittedReviewIdAsync(SubmittedHomeworkStudent submittedHomeworkStudent, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(SubmittedHomeworkStudent submittedHomeworkStudent, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SubmittedHomeworkReviewerMark>> ListSubmittedReviewMarksAsync(
        HomeworkId homeworkId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SubmittedReview>> ListBySubmittedHomeworkAsync(
        SubmittedHomeworkId submittedHomeworkId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SubmittedHomeworkId>> ListReviewedByAsync(
        StudentId studentId,
        HomeworkId homeworkId,
        CancellationToken cancellationToken);
}
