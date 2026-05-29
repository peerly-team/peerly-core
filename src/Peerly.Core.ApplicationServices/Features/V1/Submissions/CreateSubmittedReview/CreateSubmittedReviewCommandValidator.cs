using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.Shared.Validators;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;

internal sealed class CreateSubmittedReviewCommandValidator : ICommandValidator<CreateSubmittedReviewCommand, CreateSubmittedReviewCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IClock _clock;

    public CreateSubmittedReviewCommandValidator(ICommonUnitOfWorkFactory unitOfWorkFactory, IClock clock)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandValidationResult> ValidateAsync(CreateSubmittedReviewCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedHomework = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, cancellationToken);
        if (submittedHomework is null)
        {
            return OtherError.NotFound(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);
        }

        var submittedHomeworkStudent = new SubmittedHomeworkStudent { SubmittedHomeworkId = command.SubmittedHomeworkId, StudentId = command.StudentId };
        var isAssignedReviewer = await unitOfWork.ReadOnlyDistributionReviewerRepository.ExistsAsync(submittedHomeworkStudent, cancellationToken);
        if (!isAssignedReviewer)
        {
            return OtherError.PermissionDenied();
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

        var alreadyReviewed = await unitOfWork.ReadOnlySubmittedReviewRepository.ExistsAsync(submittedHomeworkStudent, cancellationToken);
        if (alreadyReviewed)
        {
            return OtherError.Conflict();
        }

        if (homework.RubricId is null)
        {
            return OtherError.NotFound(HomeworkErrors.HomeworkRubricNotFound);
        }
        var criteria = await unitOfWork.ReadOnlyRubricCriterionRepository.ListByRubricIdAsync(homework.RubricId.Value, cancellationToken);
        var scoresValidation = SubmittedReviewCommonValidator.ValidateScoresAgainstCriteria(command.Scores, criteria);

        return scoresValidation ?? CommandValidationResult.Ok();
    }
}
