using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Rubrics;

namespace Peerly.Core.Abstractions.Repositories;

public interface IRubricRepository : IReadOnlyRubricRepository
{
    Task<RubricId> AddAsync(RubricAddItem item, CancellationToken cancellationToken);
    Task DeleteAsync(RubricId rubricId, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        RubricId rubricId,
        Action<IUpdateBuilder<RubricUpdateItem>> configureUpdate,
        CancellationToken cancellationToken);
}

public interface IReadOnlyRubricRepository
{
    Task<Rubric?> GetAsync(RubricId rubricId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Rubric>> ListByTeacherAsync(TeacherId teacherId, CancellationToken cancellationToken);
}
