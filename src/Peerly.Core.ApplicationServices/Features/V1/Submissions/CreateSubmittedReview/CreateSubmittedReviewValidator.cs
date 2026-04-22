using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;

internal sealed class CreateSubmittedReviewValidator : ICreateSubmittedReviewValidator
{
    public async Task<OtherError?> ValidateAsync(
        ICommonUnitOfWork unitOfWork,
        CreateSubmittedReviewCommand command,
        CancellationToken cancellationToken)
    {
        var submittedHomeworkExists = await unitOfWork.SubmittedHomeworkRepository.ExistsAsync(command.SubmittedHomeworkId, cancellationToken);
        if (!submittedHomeworkExists)
        {
            return OtherError.NotFound();
        }

        var submittedHomeworkStudent = new SubmittedHomeworkStudent
        {
            SubmittedHomeworkId = command.SubmittedHomeworkId,
            StudentId = command.StudentId
        };

        var isAssignedReviewer = await unitOfWork.DistributionReviewerRepository.ExistsAsync(submittedHomeworkStudent, cancellationToken);
        if (!isAssignedReviewer)
        {
            return OtherError.PermissionDenied();
        }

        var alreadyReviewed = await unitOfWork.SubmittedReviewRepository.ExistsAsync(submittedHomeworkStudent, cancellationToken);
        if (alreadyReviewed)
        {
            return OtherError.Conflict();
        }

        return null;
    }
}
