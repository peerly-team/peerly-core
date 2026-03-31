using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.BackgroundService.HomeworkDistributions;

namespace Peerly.Core.Abstractions.Repositories;

public interface IHomeworkDistributionRepository : IReadOnlyHomeworkDistributionRepository
{
    Task<IReadOnlyCollection<HomeworkDistributionJobItem>> TakeAsync(
        HomeworkDistributionFilter filter,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        HomeworkId homeworkId,
        Action<IUpdateBuilder<HomeworkDistributionUpdateItem>> configureUpdate,
        CancellationToken cancellationToken);
}

public interface IReadOnlyHomeworkDistributionRepository
{
}
