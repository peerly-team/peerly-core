using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.Abstractions.Repositories;

public interface ISubmittedHomeworkFileRepository : IReadOnlySubmittedHomeworkFileRepository
{
    Task<bool> AddAsync(SubmittedHomeworkFileAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlySubmittedHomeworkFileRepository
{

}
