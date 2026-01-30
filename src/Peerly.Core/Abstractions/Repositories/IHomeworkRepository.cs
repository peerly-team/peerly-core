using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.Abstractions.Repositories;

public interface IHomeworkRepository : IReadOnlyHomeworkRepository
{

}

public interface IReadOnlyHomeworkRepository
{
    Task<IReadOnlyCollection<CourseHomeworkCount>> ListCourseHomeworkCountsAsync(
        IEnumerable<long> courseIds,
        CancellationToken cancellationToken);
}
