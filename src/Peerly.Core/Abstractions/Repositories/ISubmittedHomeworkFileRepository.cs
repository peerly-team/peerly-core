using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.Abstractions.Repositories;

public interface ISubmittedHomeworkFileRepository : IReadOnlySubmittedHomeworkFileRepository
{
    Task<bool> AddAsync(SubmittedHomeworkFileAddItem item, CancellationToken cancellationToken);

    Task DeleteBySubmittedHomeworkAsync(SubmittedHomeworkId submittedHomeworkId, CancellationToken cancellationToken);

    Task DeleteAsync(SubmittedHomeworkId submittedHomeworkId, FileId fileId, CancellationToken cancellationToken);
}

public interface IReadOnlySubmittedHomeworkFileRepository
{
    Task<FileId?> GetAnonymizedFileIdAsync(FileId fileId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<File>> ListBySubmittedHomeworkAsync(SubmittedHomeworkId submittedHomeworkId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<File>> ListAnonymizedBySubmittedHomeworkAsync(SubmittedHomeworkId submittedHomeworkId, CancellationToken cancellationToken);
}
