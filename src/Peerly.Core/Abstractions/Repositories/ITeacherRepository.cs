using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Models.Teachers;

namespace Peerly.Core.Abstractions.Repositories;

public interface ITeacherRepository : IReadOnlyTeacherRepository
{
    Task<bool> AddIfNotExistsAsync(TeacherAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlyTeacherRepository
{
    Task<IReadOnlyCollection<Teacher>> ListAsync(TeacherFilter filter, CancellationToken cancellationToken);
}
