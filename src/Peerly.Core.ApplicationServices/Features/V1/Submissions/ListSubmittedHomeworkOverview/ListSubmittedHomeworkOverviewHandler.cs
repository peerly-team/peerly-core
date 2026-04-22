using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Students;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.ListSubmittedHomeworkOverview;

internal sealed class ListSubmittedHomeworkOverviewHandler
    : IQueryHandler<ListSubmittedHomeworkOverviewQuery, ListSubmittedHomeworkOverviewQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public ListSubmittedHomeworkOverviewHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<ListSubmittedHomeworkOverviewQueryResponse> ExecuteAsync(ListSubmittedHomeworkOverviewQuery query, CancellationToken cancellationToken)
    {
        // TODO: permission — TeacherId должен вести курс, к которому относится HomeworkId.
        // Несоответствие → NotFoundException (единый паттерн с ListAssignedReviewsHandler,
        // закрыть одним PR).
        // TODO: статус — возможно ограничить выдачу только homework'ами в статусе
        // Reviewing/Closed (до этого marks не агрегированы, overview бессмысленен).
        // Сейчас пустой список возвращается автоматически (благодаря INNER-join-по-mark).
        // TODO: вынести в IListSubmittedHomeworkOverviewValidator + Validator + Installer
        // при появлении 2+ проверок, см. feedback_validator_extraction.

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedHomeworkFilter = new SubmittedHomeworkFilter { HomeworkIds = [query.HomeworkId] };
        var submittedHomeworkStudents = await unitOfWork.ReadOnlySubmittedHomeworkRepository.ListSubmittedHomeworkStudentAsync(submittedHomeworkFilter, cancellationToken);
        if (submittedHomeworkStudents.Count == 0)
        {
            return new ListSubmittedHomeworkOverviewQueryResponse { SubmittedHomeworkOverviews = [] };
        }

        var submittedHomeworkMark = await unitOfWork.ReadOnlySubmittedHomeworkMarkRepository.ListAsync(query.HomeworkId, cancellationToken);
        var submittedHomeworkMarkById = submittedHomeworkMark.ToDictionary(mark => mark.SubmittedHomeworkId);

        var submittedReviewMarks = await unitOfWork.ReadOnlySubmittedReviewRepository.ListSubmittedReviewMarksAsync(query.HomeworkId, cancellationToken);
        var reviewCountById = submittedReviewMarks
            .GroupBy(reviewerMark => reviewerMark.SubmittedHomeworkId)
            .ToDictionary(group => group.Key, group => group.Count());

        var studentFilter = new StudentFilter { StudentIds = submittedHomeworkStudents.ToArrayBy(item => item.StudentId) };
        var students = await unitOfWork.ReadOnlyStudentRepository.ListAsync(studentFilter, cancellationToken);
        var studentById = students.ToDictionary(student => student.Id);

        return new ListSubmittedHomeworkOverviewQueryResponse
        {
            SubmittedHomeworkOverviews = submittedHomeworkStudents
                .Where(item => submittedHomeworkMarkById.ContainsKey(item.SubmittedHomeworkId))
                .ToArrayBy(
                    item => item.ToSubmittedHomeworkOverview(
                        studentById[item.StudentId],
                        submittedHomeworkMarkById[item.SubmittedHomeworkId],
                        reviewCountById.GetValueOrDefault(item.SubmittedHomeworkId, 0)))
        };
    }
}
