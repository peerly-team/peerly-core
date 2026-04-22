using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PostponeHomeworkDeadlines.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.PostponeHomeworkDeadlines;

internal sealed class PostponeHomeworkDeadlinesValidator : IPostponeHomeworkDeadlinesValidator
{
    public async Task<OtherError?> ValidateAsync(
        ICommonUnitOfWork unitOfWork,
        PostponeHomeworkDeadlinesCommand command,
        CancellationToken cancellationToken)
    {
        var homework = await unitOfWork.HomeworkRepository.GetAsync(command.HomeworkId, cancellationToken);
        if (homework is null)
        {
            return OtherError.NotFound();
        }

        if (homework.TeacherId != command.TeacherId)
        {
            return OtherError.PermissionDenied();
        }

        if (homework.Status is not (HomeworkStatus.Published or HomeworkStatus.Reviewing))
        {
            return OtherError.Conflict();
        }

        if (command.Deadline is not null)
        {
            if (homework.Status is not HomeworkStatus.Published)
            {
                return OtherError.Conflict("Cannot postpone submission deadline after reviewing has started");
            }

            if (command.Deadline <= homework.Deadline)
            {
                return OtherError.Conflict("New deadline must be later than the current one");
            }
        }

        if (command.ReviewDeadline is not null && command.ReviewDeadline <= homework.ReviewDeadline)
        {
            return OtherError.Conflict("New review deadline must be later than the current one");
        }

        var effectiveDeadline = command.Deadline ?? homework.Deadline;
        var effectiveReviewDeadline = command.ReviewDeadline ?? homework.ReviewDeadline;
        if (effectiveDeadline >= effectiveReviewDeadline)
        {
            return OtherError.Conflict("Deadline must be earlier than review deadline");
        }

        return null;
    }
}
