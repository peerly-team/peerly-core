using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.ListTeacherCourseGroups;

internal sealed class ListTeacherCourseGroupsHandler : IQueryHandler<ListTeacherCourseGroupsQuery, ListTeacherCourseGroupsQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public ListTeacherCourseGroupsHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<ListTeacherCourseGroupsQueryResponse> ExecuteAsync(
        ListTeacherCourseGroupsQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var groupFilter = query.ToGroupFilter();
        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(groupFilter, cancellationToken);

        await EnsureTeacherHasAccessAsync(unitOfWork, query, groups, cancellationToken);

        return new ListTeacherCourseGroupsQueryResponse
        {
            Groups = groups
        };
    }

    private static async Task EnsureTeacherHasAccessAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        ListTeacherCourseGroupsQuery query,
        IReadOnlyCollection<Group> groups,
        CancellationToken cancellationToken)
    {
        var courseTeacher = query.ToCourseTeacher();
        var isCourseTeacher = await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, cancellationToken);
        if (isCourseTeacher)
        {
            return;
        }

        if (groups.Count == 0)
        {
            throw new NotFoundException();
        }

        var groupTeacherFilter = query.ToGroupTeacherFilter(groups);
        var groupTeachers = await unitOfWork.ReadOnlyGroupTeacherRepository.ListAsync(groupTeacherFilter, cancellationToken);
        if (groupTeachers.Count == 0)
        {
            throw new NotFoundException();
        }
    }
}
