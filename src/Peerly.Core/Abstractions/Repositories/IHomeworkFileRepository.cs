using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.Abstractions.Repositories;

public interface IHomeworkFileRepository : IReadOnlyHomeworkFileRepository
{
    Task<bool> AddAsync(HomeworkFileAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlyHomeworkFileRepository
{

}
