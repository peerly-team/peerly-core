using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.ListStudentCourseGroups;

internal sealed class ListStudentCourseGroupsHandler : IQueryHandler<ListStudentCourseGroupsQuery, ListStudentCourseGroupsQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public ListStudentCourseGroupsHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<ListStudentCourseGroupsQueryResponse> ExecuteAsync(
        ListStudentCourseGroupsQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var groupFilter = query.ToGroupFilter();
        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(groupFilter, cancellationToken);

        await EnsureStudentHasAccessAsync(unitOfWork, query, groups, cancellationToken);

        return new ListStudentCourseGroupsQueryResponse
        {
            Groups = groups
        };
    }

    private static async Task EnsureStudentHasAccessAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        ListStudentCourseGroupsQuery query,
        IReadOnlyCollection<Group> groups,
        CancellationToken cancellationToken)
    {
        if (groups.Count == 0)
        {
            throw new NotFoundException();
        }

        var groupStudentFilter = query.ToGroupStudentFilter(groups);
        var groupStudents = await unitOfWork.ReadOnlyGroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);
        if (groupStudents.Count == 0)
        {
            throw new NotFoundException();
        }
    }
}
