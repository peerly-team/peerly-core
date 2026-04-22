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

    public async Task<GetSubmittedReviewQueryResponse> ExecuteAsync(
        GetSubmittedReviewQuery query,
        CancellationToken cancellationToken)
    {
        // TODO: permission — StudentId должен быть автором рецензии SubmittedReviewId.
        // Несоответствие → NotFoundException (скрываем PermissionDenied, чтобы не раскрывать
        // существование чужой рецензии; стандартный паттерн Query, образец — GetSubmittedHomeworkHandler).
        // Потребуется сравнение query.StudentId с SubmittedReview.StudentId — либо расширить
        // доменную модель SubmittedReview полем StudentId, либо добавить тонкий
        // GetAuthorAsync(SubmittedReviewId) -> StudentId?.
        // TODO: вынести проверки в IGetSubmittedReviewValidator + GetSubmittedReviewValidator
        // + GetSubmittedReviewInstaller, если проверок окажется больше одной.

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedReview = await unitOfWork.ReadOnlySubmittedReviewRepository.GetAsync(query.SubmittedReviewId, cancellationToken)
                              ?? throw new NotFoundException();

        return new GetSubmittedReviewQueryResponse
        {
            SubmittedReview = submittedReview
        };
    }
}
