using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetAssignedReview;

internal sealed class GetAssignedReviewHandler : IQueryHandler<GetAssignedReviewQuery, GetAssignedReviewQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IQueryValidator<GetAssignedReviewQuery, GetAssignedReviewQueryResponse> _validator;

    public GetAssignedReviewHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IQueryValidator<GetAssignedReviewQuery, GetAssignedReviewQueryResponse> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<GetAssignedReviewQueryResponse> ExecuteAsync(GetAssignedReviewQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(query, cancellationToken);

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submission = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(query.SubmittedHomeworkId, cancellationToken)
                         ?? throw new NotFoundException();
        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(submission.HomeworkId, cancellationToken)
                       ?? throw new NotFoundException();
        var files = await unitOfWork.ReadOnlySubmittedHomeworkFileRepository.ListAnonymizedBySubmittedHomeworkAsync(query.SubmittedHomeworkId, cancellationToken);

        var submittedHomeworkStudent = query.ToSubmittedHomeworkStudent();
        var submittedReviewId = await unitOfWork.ReadOnlySubmittedReviewRepository.GetIdAsync(submittedHomeworkStudent, cancellationToken);

        return new GetAssignedReviewQueryResponse
        {
            SubmittedHomeworkId = submission.Id,
            Comment = submission.Comment,
            RubricId = homework.RubricId,
            Files = files,
            SubmittedReviewId = submittedReviewId
        };
    }
}
