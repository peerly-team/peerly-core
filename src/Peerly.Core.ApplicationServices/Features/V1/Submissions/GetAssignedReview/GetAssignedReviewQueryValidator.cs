using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetAssignedReview;

internal sealed class GetAssignedReviewQueryValidator : IQueryValidator<GetAssignedReviewQuery, GetAssignedReviewQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IClock _clock;

    public GetAssignedReviewQueryValidator(ICommonUnitOfWorkFactory unitOfWorkFactory, IClock clock)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _clock = clock;
    }

    public async Task ValidateAsync(GetAssignedReviewQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedHomeworkStudent = query.ToSubmittedHomeworkStudent();
        var isAssignedReviewer = await unitOfWork.ReadOnlyDistributionReviewerRepository.ExistsAsync(submittedHomeworkStudent, cancellationToken);
        if (!isAssignedReviewer)
        {
            throw new NotFoundException();
        }

        var submittedHomework = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(query.SubmittedHomeworkId, cancellationToken)
                                ?? throw new NotFoundException();
        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, cancellationToken)
                       ?? throw new NotFoundException();

        if (homework.Status is not HomeworkStatus.Reviewing || _clock.GetCurrentTime() >= homework.ReviewDeadline)
        {
            throw new BusinessValidationException(HomeworkErrors.HomeworkNotAcceptingReviews);
        }
    }
}
