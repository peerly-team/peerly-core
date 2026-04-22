using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using SubmittedHomeworkStudent = Peerly.Core.Models.Submissions.SubmittedHomeworkStudent;

namespace Peerly.Core.Abstractions.Repositories;

public interface IDistributionReviewerRepository : IReadOnlyDistributionReviewerRepository
{
    Task BatchAddAsync(IReadOnlyCollection<DistributionReviewerAddItem> items, CancellationToken cancellationToken);
}

public interface IReadOnlyDistributionReviewerRepository
{
    Task<IReadOnlyCollection<SubmittedHomeworkId>> ListAssignedByAsync(StudentId studentId, HomeworkId homeworkId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(SubmittedHomeworkStudent submittedHomeworkStudent, CancellationToken cancellationToken);
}
