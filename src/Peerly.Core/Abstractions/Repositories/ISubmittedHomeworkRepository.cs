using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.Abstractions.Repositories;

public interface ISubmittedHomeworkRepository : IReadOnlySubmittedHomeworkRepository
{
    Task<SubmittedHomeworkId> AddAsync(SubmittedHomeworkAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlySubmittedHomeworkRepository
{

}
