using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.GetStudentGroup;

internal sealed class GetStudentGroupHandler : IQueryHandler<GetStudentGroupQuery, GetStudentGroupQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetStudentGroupHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetStudentGroupQueryResponse> ExecuteAsync(GetStudentGroupQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var group = await unitOfWork.ReadOnlyGroupRepository.GetAsync(query.GroupId, cancellationToken)
                    ?? throw new NotFoundException();

        await EnsureStudentHasAccessAsync(unitOfWork, query, group, cancellationToken);

        return new GetStudentGroupQueryResponse
        {
            Group = group
        };
    }

    private static async Task EnsureStudentHasAccessAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        GetStudentGroupQuery query,
        Group group,
        CancellationToken cancellationToken)
    {
        var groupFilter = query.ToGroupFilter(group.CourseId);
        var courseGroups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(groupFilter, cancellationToken);

        var groupStudentFilter = query.ToGroupStudentFilter(courseGroups);
        var groupStudents = await unitOfWork.ReadOnlyGroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);
        if (groupStudents.Count == 0)
        {
            throw new NotFoundException();
        }
    }
}
