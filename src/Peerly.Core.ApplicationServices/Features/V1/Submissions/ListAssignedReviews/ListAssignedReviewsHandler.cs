using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.ListAssignedReviews;

internal sealed class ListAssignedReviewsHandler : IQueryHandler<ListAssignedReviewsQuery, ListAssignedReviewsQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IQueryValidator<ListAssignedReviewsQuery, ListAssignedReviewsQueryResponse> _validator;

    public ListAssignedReviewsHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IQueryValidator<ListAssignedReviewsQuery, ListAssignedReviewsQueryResponse> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<ListAssignedReviewsQueryResponse> ExecuteAsync(ListAssignedReviewsQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(query, cancellationToken);

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var homeworkStudent = query.ToHomeworkStudent();
        var assignedIds = await unitOfWork.ReadOnlyDistributionReviewerRepository.ListAssignedByAsync(homeworkStudent, cancellationToken);
        var reviewedIds = await unitOfWork.ReadOnlySubmittedReviewRepository.ListReviewedByAsync(homeworkStudent, cancellationToken);

        var reviewedSet = reviewedIds.ToHashSet();
        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(query.HomeworkId, cancellationToken);
        var assignedReviews = assignedIds.ToArrayBy(
            id => new AssignedReview
            {
                SubmittedHomeworkId = id,
                HomeworkName = homework!.Name,
                IsReviewed = reviewedSet.Contains(id)
            });

        return new ListAssignedReviewsQueryResponse { AssignedReviews = assignedReviews };
    }
}
