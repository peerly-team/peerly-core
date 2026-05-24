using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.ListSubmittedHomeworkOverview;

internal sealed class ListSubmittedHomeworkOverviewHandler
    : IQueryHandler<ListSubmittedHomeworkOverviewQuery, ListSubmittedHomeworkOverviewQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IQueryValidator<ListSubmittedHomeworkOverviewQuery, ListSubmittedHomeworkOverviewQueryResponse> _validator;

    public ListSubmittedHomeworkOverviewHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IQueryValidator<ListSubmittedHomeworkOverviewQuery, ListSubmittedHomeworkOverviewQueryResponse> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<ListSubmittedHomeworkOverviewQueryResponse> ExecuteAsync(ListSubmittedHomeworkOverviewQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(query, cancellationToken);

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedHomeworkFilter = new SubmittedHomeworkFilter { HomeworkIds = [query.HomeworkId] };
        var submittedHomeworkStudents = await unitOfWork.ReadOnlySubmittedHomeworkRepository.ListSubmittedHomeworkStudentAsync(submittedHomeworkFilter, cancellationToken);
        if (submittedHomeworkStudents.Count == 0)
        {
            return new ListSubmittedHomeworkOverviewQueryResponse { SubmittedHomeworkOverviews = [] };
        }

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(query.HomeworkId, cancellationToken);
        var submittedHomeworkMarkById = homework!.Status is HomeworkStatus.Reviewing
            ? new Dictionary<SubmittedHomeworkId, SubmittedHomeworkMark>()
            : await GetSubmittedHomeworkMarkByIdAsync(unitOfWork, query.HomeworkId, cancellationToken);

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
                .ToArrayBy(
                    item => item.ToSubmittedHomeworkOverview(
                        studentById[item.StudentId],
                        submittedHomeworkMarkById.GetValueOrDefault(item.SubmittedHomeworkId),
                        reviewCountById.GetValueOrDefault(item.SubmittedHomeworkId, 0)))
        };
    }

    private async Task<Dictionary<SubmittedHomeworkId, SubmittedHomeworkMark>> GetSubmittedHomeworkMarkByIdAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        HomeworkId homeworkId,
        CancellationToken cancellationToken)
    {
        var submittedHomeworkMarks = await unitOfWork.ReadOnlySubmittedHomeworkMarkRepository.ListAsync(homeworkId, cancellationToken);
        return submittedHomeworkMarks.ToDictionary(mark => mark.SubmittedHomeworkId);
    }
}
