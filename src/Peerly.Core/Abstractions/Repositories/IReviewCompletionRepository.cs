using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.BackgroundService.ReviewCompletions;

namespace Peerly.Core.Abstractions.Repositories;

public interface IReviewCompletionRepository : IReadOnlyReviewCompletionRepository
{
    Task AddAsync(ReviewCompletionAddItem item, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReviewCompletionJobItem>> TakeAsync(
        ReviewCompletionFilter filter,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        HomeworkId homeworkId,
        Action<IUpdateBuilder<ReviewCompletionUpdateItem>> configureUpdate,
        CancellationToken cancellationToken);
}

public interface IReadOnlyReviewCompletionRepository
{
}
