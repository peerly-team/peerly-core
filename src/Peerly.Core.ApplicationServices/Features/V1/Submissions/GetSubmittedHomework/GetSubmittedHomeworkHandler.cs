using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmittedHomework;

internal sealed class GetSubmittedHomeworkHandler : IQueryHandler<GetSubmittedHomeworkQuery, GetSubmittedHomeworkQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetSubmittedHomeworkHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetSubmittedHomeworkQueryResponse> ExecuteAsync(
        GetSubmittedHomeworkQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        // TODO: реализовать permission-check по образцу GetStudentHomeworkHandler.EnsureStudentHasAccessAsync:
        // 1. Ownership: submission.StudentId != query.StudentId → throw new NotFoundException()
        //    (скрываем PermissionDenied, чтобы не раскрывать существование чужой отправки)
        // 2. IStudentCourseAccessChecker — студент имеет доступ к курсу homework'а
        // 3. Если homework привязан к группе — студент состоит в этой группе
        // Все несоответствия возвращают NotFoundException (без сообщения, стандартный паттерн Query).

        var submission = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(query.SubmittedHomeworkId, cancellationToken)
                         ?? throw new NotFoundException();
        var files = await unitOfWork.ReadOnlySubmittedHomeworkFileRepository.ListBySubmittedHomeworkAsync(query.SubmittedHomeworkId, cancellationToken);
        var reviews = await unitOfWork.ReadOnlySubmittedReviewRepository.ListBySubmittedHomeworkAsync(query.SubmittedHomeworkId, cancellationToken);
        var mark = await unitOfWork.ReadOnlySubmittedHomeworkMarkRepository.GetBySubmittedHomeworkAsync(query.SubmittedHomeworkId, cancellationToken);

        return new GetSubmittedHomeworkQueryResponse
        {
            Submission = submission,
            Files = files,
            Reviews = reviews,
            FinalMark = mark?.TeacherMark ?? mark?.ReviewersMark
        };
    }
}
