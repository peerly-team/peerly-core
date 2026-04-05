using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Models.Students;

namespace Peerly.Core.Abstractions.Repositories;

public interface IStudentRepository : IReadOnlyStudentRepository
{
    Task<bool> AddIfNotExistsAsync(StudentAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlyStudentRepository
{
    Task<IReadOnlyCollection<Student>> ListAsync(StudentFilter filter, CancellationToken cancellationToken);
}
