using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Pagination;

namespace Peerly.Core.Abstractions.Repositories;

public interface ICourseRepository : IReadOnlyCourseRepository
{
    Task<CourseId> AddAsync(CourseAddItem item, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        CourseId courseId,
        Action<IUpdateBuilder<CourseUpdateItem>> configureUpdate,
        CancellationToken cancellationToken);
}

public interface IReadOnlyCourseRepository
{
    Task<Course?> GetAsync(CourseId courseId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Course>> ListAsync(CourseFilter filter, PaginationInfo paginationInfo, CancellationToken cancellationToken);
}
