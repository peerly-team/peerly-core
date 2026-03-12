using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchStudentCourseHomeworks;

internal sealed class SearchStudentCourseHomeworksHandler : IQueryHandler<SearchStudentCourseHomeworksQuery, SearchStudentCourseHomeworksQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public SearchStudentCourseHomeworksHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<SearchStudentCourseHomeworksQueryResponse> ExecuteAsync(
        SearchStudentCourseHomeworksQuery query,
        CancellationToken cancellationToken)
    {
        var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var groupFilter = GroupFilter.Empty() with { CourseIds = [query.CourseId] };
        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(groupFilter, cancellationToken);
        if (groups.Count == 0) { return new SearchStudentCourseHomeworksQueryResponse { Homeworks = [] }; }

        var groupStudentFilter = new GroupStudentFilter
        {
            GroupIds = groups.ToArrayBy(group => group.Id),
            StudentIds = [query.StudentId]
        };
        var groupStudents = await unitOfWork.ReadOnlyGroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);
        if (groupStudents.Count == 0) { throw new NotFoundException(); }

        var homeworkFilter = new HomeworkFilter
        {
            CourseIds = [query.CourseId],
            GroupIds = groupStudents.ToArrayBy(groupStudent => groupStudent.GroupId),
            HomeworkStatuses = [HomeworkStatus.Published, HomeworkStatus.Reviewing, HomeworkStatus.Confirmation, HomeworkStatus.Finished]
        };
        var homeworks = await unitOfWork.ReadOnlyHomeworkRepository.ListAsync(homeworkFilter, cancellationToken);

        return new SearchStudentCourseHomeworksQueryResponse
        {
            Homeworks = homeworks
        };
    }
}
