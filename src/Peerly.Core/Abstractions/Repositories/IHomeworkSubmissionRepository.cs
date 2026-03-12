using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.Abstractions.Repositories;

public interface IHomeworkSubmissionRepository : IReadOnlyHomeworkSubmissionRepository
{
    Task<HomeworkSubmissionId> AddAsync(HomeworkSubmissionAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlyHomeworkSubmissionRepository
{

}
