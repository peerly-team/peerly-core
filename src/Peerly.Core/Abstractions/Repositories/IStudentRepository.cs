using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Students;

namespace Peerly.Core.Abstractions.Repositories;

public interface IStudentRepository : IReadOnlyStudentRepository
{
    Task<bool> AddIfNotExistsAsync(StudentAddItem item, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        StudentId studentId,
        Action<IUpdateBuilder<StudentUpdateItem>> configureUpdate,
        CancellationToken cancellationToken);
}

public interface IReadOnlyStudentRepository
{
    Task<Student?> GetAsync(StudentId studentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Student>> ListAsync(StudentFilter filter, CancellationToken cancellationToken);
}
