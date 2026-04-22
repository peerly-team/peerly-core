using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmittedHomework;

internal sealed class GetTeacherSubmittedHomeworkHandler
    : IQueryHandler<GetTeacherSubmittedHomeworkQuery, GetTeacherSubmittedHomeworkQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetTeacherSubmittedHomeworkHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetTeacherSubmittedHomeworkQueryResponse> ExecuteAsync(
        GetTeacherSubmittedHomeworkQuery query,
        CancellationToken cancellationToken)
    {
        // TODO: permission — TeacherId должен вести курс, к которому относится HomeworkId сдачи
        // (homework_id берётся из submittedHomework.HomeworkId). Несоответствие → NotFoundException
        // (единый паттерн с ListSubmittedHomeworkOverviewHandler, закрыть одним PR).
        // TODO: статус — возможно ограничить выдачу только homework'ами в статусе Reviewing/Closed.
        // Сейчас запрос закрыт INNER-семантикой через GetBySubmittedHomeworkAsync == null → NotFound.
        // TODO: вынести в IGetTeacherSubmittedHomeworkValidator + Validator + Installer
        // при появлении 2+ проверок, см. feedback_validator_extraction.

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedHomeworkId = query.SubmittedHomeworkId;
        var submittedHomework = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(submittedHomeworkId, cancellationToken)
                                ?? throw new NotFoundException();

        var files = await unitOfWork.ReadOnlySubmittedHomeworkFileRepository.ListBySubmittedHomeworkAsync(submittedHomeworkId, cancellationToken);
        var reviews = await unitOfWork.ReadOnlySubmittedReviewRepository.ListBySubmittedHomeworkAsync(submittedHomeworkId, cancellationToken);
        var submittedHomeworkMark = await unitOfWork.ReadOnlySubmittedHomeworkMarkRepository.GetBySubmittedHomeworkAsync(submittedHomeworkId, cancellationToken)
                                    ?? throw new NotFoundException();

        var studentById = await GetStudentByIdAsync(unitOfWork, reviews, submittedHomework.StudentId, cancellationToken);
        return new GetTeacherSubmittedHomeworkQueryResponse
        {
            SubmittedHomework = submittedHomework,
            Student = studentById[submittedHomework.StudentId],
            Files = files,
            SubmittedReviews = reviews.ToArrayBy(review => review.ToTeacherSubmittedReview(studentById[review.StudentId])),
            ReviewersMark = submittedHomeworkMark.ReviewersMark,
            TeacherMark = submittedHomeworkMark.TeacherMark
        };
    }

    private static async Task<Dictionary<StudentId, Student>> GetStudentByIdAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        IReadOnlyCollection<SubmittedReview> reviews,
        StudentId studentId,
        CancellationToken cancellationToken)
    {
        var studentIds = reviews
            .Select(review => review.StudentId)
            .Append(studentId)
            .ToArray();

        var studentFilter = new StudentFilter { StudentIds = studentIds };
        var students = await unitOfWork.ReadOnlyStudentRepository.ListAsync(studentFilter, cancellationToken);

        return students.ToDictionary(student => student.Id);
    }
}
