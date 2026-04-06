using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.Abstractions.Repositories;

public interface ISubmittedHomeworkFileRepository : IReadOnlySubmittedHomeworkFileRepository
{
    Task<bool> AddAsync(SubmittedHomeworkFileAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlySubmittedHomeworkFileRepository
{
    Task<FileId?> GetAnonymizedFileIdAsync(FileId fileId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SubmittedHomeworkFileItem>> ListAsync(SubmittedHomeworkId submittedHomeworkId, CancellationToken cancellationToken);
}
