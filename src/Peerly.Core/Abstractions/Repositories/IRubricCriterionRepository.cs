using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Rubrics;

namespace Peerly.Core.Abstractions.Repositories;

public interface IRubricCriterionRepository : IReadOnlyRubricCriterionRepository
{
    Task BatchAddAsync(IReadOnlyCollection<RubricCriterionAddItem> items, CancellationToken cancellationToken);
    Task DeleteByRubricIdAsync(RubricId rubricId, CancellationToken cancellationToken);
}

public interface IReadOnlyRubricCriterionRepository
{
    Task<IReadOnlyCollection<RubricCriterion>> ListByRubricIdAsync(RubricId rubricId, CancellationToken cancellationToken);
}
