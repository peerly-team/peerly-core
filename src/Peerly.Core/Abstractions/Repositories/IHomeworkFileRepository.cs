using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.Abstractions.Repositories;

public interface IHomeworkFileRepository : IReadOnlyHomeworkFileRepository
{
    Task<bool> AddAsync(HomeworkFileAddItem item, CancellationToken cancellationToken);
    Task DeleteAsync(HomeworkId homeworkId, FileId fileId, CancellationToken cancellationToken);
    Task DeleteByHomeworkAsync(HomeworkId homeworkId, CancellationToken cancellationToken);
}

public interface IReadOnlyHomeworkFileRepository
{
    Task<IReadOnlyCollection<File>> ListFilesAsync(HomeworkId homeworkId, CancellationToken cancellationToken);
}
