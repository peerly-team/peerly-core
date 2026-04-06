using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetStudentCourseResults;

internal sealed class GetStudentCourseResultsHandler : IQueryHandler<GetStudentCourseResultsQuery, GetStudentCourseResultsQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetStudentCourseResultsHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetStudentCourseResultsQueryResponse> ExecuteAsync(
        GetStudentCourseResultsQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var groupFilter = GroupFilter.Empty() with { CourseIds = [query.CourseId] };
        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(groupFilter, cancellationToken);
        if (groups.Count == 0)
        {
            return new GetStudentCourseResultsQueryResponse { Results = [] };
        }

        var groupStudentFilter = new GroupStudentFilter
        {
            GroupIds = groups.ToArrayBy(g => g.Id),
            StudentIds = [query.StudentId]
        };
        var groupStudents = await unitOfWork.ReadOnlyGroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);
        if (groupStudents.Count == 0)
        {
            throw new NotFoundException();
        }

        var homeworkFilter = new HomeworkFilter
        {
            CourseIds = [query.CourseId],
            GroupIds = groupStudents.ToArrayBy(gs => gs.GroupId),
            HomeworkStatuses = [HomeworkStatus.Published, HomeworkStatus.Reviewing, HomeworkStatus.Confirmation, HomeworkStatus.Finished]
        };
        var homeworks = await unitOfWork.ReadOnlyHomeworkRepository.ListAsync(homeworkFilter, cancellationToken);

        if (homeworks.Count == 0)
        {
            return new GetStudentCourseResultsQueryResponse { Results = [] };
        }

        var results = new List<StudentHomeworkResultItem>(homeworks.Count);
        foreach (var homework in homeworks)
        {
            int? reviewersMark = null;
            int? teacherMark = null;

            var submission = await unitOfWork.ReadOnlySubmittedHomeworkRepository
                .GetByHomeworkAndStudentAsync(homework.Id, query.StudentId, cancellationToken);

            if (submission is not null)
            {
                var mark = await unitOfWork.ReadOnlySubmittedHomeworkMarkRepository
                    .GetAsync(submission.Id, cancellationToken);
                if (mark is not null)
                {
                    reviewersMark = mark.ReviewersMark;
                    teacherMark = mark.TeacherMark;
                }
            }

            results.Add(new StudentHomeworkResultItem
            {
                HomeworkId = (long)homework.Id,
                HomeworkName = homework.Name,
                HomeworkStatus = homework.Status,
                ReviewersMark = reviewersMark,
                TeacherMark = teacherMark
            });
        }

        return new GetStudentCourseResultsQueryResponse
        {
            Results = results
        };
    }
}
