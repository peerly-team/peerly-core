using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Files;

namespace Peerly.Core.Abstractions.Repositories;

public interface ICourseFileRepository : IReadOnlyCourseFileRepository
{
    Task<bool> AddAsync(CourseFileAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlyCourseFileRepository
{
    Task<IReadOnlyCollection<File>> ListFilesAsync(CourseId courseId, CancellationToken cancellationToken);
}
