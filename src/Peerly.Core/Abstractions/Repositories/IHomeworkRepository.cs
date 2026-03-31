using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.Abstractions.Repositories;

public interface IHomeworkRepository : IReadOnlyHomeworkRepository
{
    Task<HomeworkId> AddAsync(HomeworkAddItem item, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        HomeworkId homeworkId,
        Action<IUpdateBuilder<HomeworkUpdateItem>> configureUpdate,
        CancellationToken cancellationToken);
}

public interface IReadOnlyHomeworkRepository
{
    Task<Homework?> GetAsync(HomeworkId homeworkId, CancellationToken cancellationToken);
    Task<int> GetHomeworkCountAsync(CourseId courseId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Homework>> ListAsync(HomeworkFilter filter, CancellationToken cancellationToken);
}
