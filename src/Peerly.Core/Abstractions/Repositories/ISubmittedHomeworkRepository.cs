using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using SubmittedHomeworkStudent = Peerly.Core.Models.Homeworks.SubmittedHomeworkStudent;

namespace Peerly.Core.Abstractions.Repositories;

public interface ISubmittedHomeworkRepository : IReadOnlySubmittedHomeworkRepository
{
    Task<SubmittedHomeworkId> AddAsync(SubmittedHomeworkAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlySubmittedHomeworkRepository
{
    Task<SubmittedHomework?> GetAsync(SubmittedHomeworkId submittedHomeworkId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(SubmittedHomeworkId submittedHomeworkId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SubmittedHomeworkStudent>> ListSubmittedHomeworkStudentAsync(
        SubmittedHomeworkFilter filter,
        CancellationToken cancellationToken);
}
