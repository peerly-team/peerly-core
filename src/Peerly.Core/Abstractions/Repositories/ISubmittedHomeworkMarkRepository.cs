using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.Abstractions.Repositories;

public interface ISubmittedHomeworkMarkRepository : IReadOnlySubmittedHomeworkMarkRepository
{
    Task BatchAddAsync(IReadOnlyCollection<SubmittedHomeworkMarkAddItem> items, CancellationToken cancellationToken);

    Task BatchUpdateAsync(IReadOnlyCollection<SubmittedHomeworkMarkBatchUpdateItem> items, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        SubmittedHomeworkId submittedHomeworkId,
        Action<IUpdateBuilder<SubmittedHomeworkMarkUpdateItem>> configureUpdate,
        CancellationToken cancellationToken);
}

public interface IReadOnlySubmittedHomeworkMarkRepository
{
    Task<SubmittedHomeworkMark?> GetAsync(SubmittedHomeworkId submittedHomeworkId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SubmittedHomeworkMark>> ListAsync(
        HomeworkId homeworkId,
        CancellationToken cancellationToken);
}
