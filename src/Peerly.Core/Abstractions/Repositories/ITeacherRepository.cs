using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Teachers;

namespace Peerly.Core.Abstractions.Repositories;

public interface ITeacherRepository : IReadOnlyTeacherRepository
{
    Task<bool> AddIfNotExistsAsync(TeacherAddItem item, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        TeacherId teacherId,
        Action<IUpdateBuilder<TeacherUpdateItem>> configureUpdate,
        CancellationToken cancellationToken);
}

public interface IReadOnlyTeacherRepository
{
    Task<Teacher?> GetAsync(TeacherId teacherId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Teacher>> ListAsync(TeacherFilter filter, CancellationToken cancellationToken);
}
