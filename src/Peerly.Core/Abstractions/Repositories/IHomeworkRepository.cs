using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.Abstractions.Repositories;

public interface IHomeworkRepository : IReadOnlyHomeworkRepository
{

}

public interface IReadOnlyHomeworkRepository
{
    Task<int> GetHomeworkCountAsync(CourseId courseId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Homework>> ListAsync(HomeworkFilter filter, CancellationToken cancellationToken);
}
