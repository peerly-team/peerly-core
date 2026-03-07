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
}

public interface IReadOnlyCourseRepository
{
    Task<IReadOnlyCollection<Course>> ListAsync(CourseFilter filter, PaginationInfo paginationInfo, CancellationToken cancellationToken);
}
