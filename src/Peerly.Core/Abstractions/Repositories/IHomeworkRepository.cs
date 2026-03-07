using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.Abstractions.Repositories;

public interface IHomeworkRepository : IReadOnlyHomeworkRepository
{

}

public interface IReadOnlyHomeworkRepository
{
    Task<IReadOnlyCollection<CourseHomeworkCount>> ListCourseHomeworkCountAsync(
        HomeworkFilter filter,
        CancellationToken cancellationToken);
}
