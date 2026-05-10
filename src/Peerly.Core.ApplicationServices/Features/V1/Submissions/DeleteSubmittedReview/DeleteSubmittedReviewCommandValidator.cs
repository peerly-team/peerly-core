using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedReview;

internal sealed class DeleteSubmittedReviewCommandValidator : ICommandValidator<DeleteSubmittedReviewCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IClock _clock;

    public DeleteSubmittedReviewCommandValidator(ICommonUnitOfWorkFactory unitOfWorkFactory, IClock clock)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandValidationResult> ValidateAsync(DeleteSubmittedReviewCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedReview = await unitOfWork.ReadOnlySubmittedReviewRepository.GetAsync(command.SubmittedReviewId, cancellationToken);
        if (submittedReview is null)
        {
            return OtherError.NotFound(SubmittedReviewErrors.SubmittedReviewNotFound);
        }

        if (submittedReview.StudentId != command.StudentId)
        {
            return OtherError.PermissionDenied();
        }

        var submittedHomework = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(submittedReview.SubmittedHomeworkId, cancellationToken);
        if (submittedHomework is null)
        {
            return OtherError.NotFound(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);
        }

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, cancellationToken);
        if (homework is null)
        {
            return OtherError.NotFound(HomeworkErrors.HomeworkNotFound);
        }

        if (homework.Status is not HomeworkStatus.Reviewing || _clock.GetCurrentTime() >= homework.ReviewDeadline)
        {
            return ValidationError.From(HomeworkErrors.HomeworkNotAcceptingReviews);
        }

        return CommandValidationResult.Ok();
    }
}
