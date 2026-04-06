using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;
using SubmittedHomeworkStudent = Peerly.Core.Models.Submissions.SubmittedHomeworkStudent;

namespace Peerly.Core.Abstractions.Repositories;

public interface IDistributionReviewerRepository : IReadOnlyDistributionReviewerRepository
{
    Task BatchAddAsync(IReadOnlyCollection<DistributionReviewerAddItem> items, CancellationToken cancellationToken);
}

public interface IReadOnlyDistributionReviewerRepository
{
    Task<bool> ExistsAsync(SubmittedHomeworkStudent submittedHomeworkStudent, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AssignedReview>> ListAssignedReviewsAsync(StudentId studentId, HomeworkId homeworkId, CancellationToken cancellationToken);
}
