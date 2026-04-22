using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetAssignedReview;

internal sealed class GetAssignedReviewHandler : IQueryHandler<GetAssignedReviewQuery, GetAssignedReviewQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetAssignedReviewHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetAssignedReviewQueryResponse> ExecuteAsync(
        GetAssignedReviewQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        // TODO: permission-check — убедиться, что studentId назначен ревьюером на этот submittedHomeworkId
        // (IReadOnlyDistributionReviewerRepository.ExistsAsync(SubmittedHomeworkStudent)) — при отсутствии бросать
        // PermissionDeniedException до проверки существования работы (правило permission-before-existence).
        // TODO: проверить, что homework во «фазе ревью» и ReviewDeadline не истёк — иначе возвращать FailedPrecondition.
        // NOTE: File.Name у анонимизированной копии сейчас совпадает с исходным — возможна утечка PII
        // через имя файла (например, "Иванов_Иван_ДЗ.txt"). Решается на стороне upload/анонимайзера,
        // не здесь.

        var submission = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(query.SubmittedHomeworkId, cancellationToken)
                         ?? throw new NotFoundException();
        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(submission.HomeworkId, cancellationToken)
                       ?? throw new NotFoundException();
        var files = await unitOfWork.ReadOnlySubmittedHomeworkFileRepository.ListAnonymizedBySubmittedHomeworkAsync(query.SubmittedHomeworkId, cancellationToken);

        var submittedHomeworkStudent = query.ToSubmittedHomeworkStudent();
        var submittedReviewId = await unitOfWork.ReadOnlySubmittedReviewRepository.GetSubmittedReviewIdAsync(submittedHomeworkStudent, cancellationToken);

        return new GetAssignedReviewQueryResponse
        {
            SubmittedHomeworkId = submission.Id,
            Comment = submission.Comment,
            Checklist = homework.CheckList,
            Files = files,
            SubmittedReviewId = submittedReviewId
        };
    }
}
