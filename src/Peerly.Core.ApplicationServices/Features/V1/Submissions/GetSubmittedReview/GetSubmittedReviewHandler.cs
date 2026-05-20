using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmittedReview;

internal sealed class GetSubmittedReviewHandler : IQueryHandler<GetSubmittedReviewQuery, GetSubmittedReviewQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetSubmittedReviewHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetSubmittedReviewQueryResponse> ExecuteAsync(GetSubmittedReviewQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedReview = await unitOfWork.ReadOnlySubmittedReviewRepository.GetAsync(query.SubmittedReviewId, cancellationToken);
        if (submittedReview is null || submittedReview.StudentId != query.StudentId)
            throw new NotFoundException();

        return new GetSubmittedReviewQueryResponse
        {
            SubmittedReview = submittedReview
        };
    }
}
