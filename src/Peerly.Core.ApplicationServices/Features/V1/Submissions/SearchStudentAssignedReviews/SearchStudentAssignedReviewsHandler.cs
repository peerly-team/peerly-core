using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.SearchStudentAssignedReviews;

internal sealed class SearchStudentAssignedReviewsHandler : IQueryHandler<SearchStudentAssignedReviewsQuery, SearchStudentAssignedReviewsQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public SearchStudentAssignedReviewsHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<SearchStudentAssignedReviewsQueryResponse> ExecuteAsync(
        SearchStudentAssignedReviewsQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var assignedReviews = await unitOfWork.ReadOnlyDistributionReviewerRepository
            .ListAssignedReviewsAsync(query.StudentId, query.HomeworkId, cancellationToken);

        return new SearchStudentAssignedReviewsQueryResponse
        {
            AssignedReviews = assignedReviews
        };
    }
}
