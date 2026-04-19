using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.GetTeacherGroup;

internal sealed class GetTeacherGroupHandler : IQueryHandler<GetTeacherGroupQuery, GetTeacherGroupQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetTeacherGroupHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetTeacherGroupQueryResponse> ExecuteAsync(GetTeacherGroupQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var group = await unitOfWork.ReadOnlyGroupRepository.GetAsync(query.GroupId, cancellationToken)
                    ?? throw new NotFoundException();

        await EnsureTeacherHasAccessAsync(unitOfWork, query, group, cancellationToken);

        return new GetTeacherGroupQueryResponse
        {
            Group = group
        };
    }

    private static async Task EnsureTeacherHasAccessAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        GetTeacherGroupQuery query,
        Group group,
        CancellationToken cancellationToken)
    {
        var courseTeacher = query.ToCourseTeacher(group.CourseId);
        if (await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, cancellationToken))
        {
            return;
        }

        var groupTeacher = query.ToGroupTeacher();
        if (!await unitOfWork.ReadOnlyGroupTeacherRepository.ExistsAsync(groupTeacher, cancellationToken))
        {
            throw new NotFoundException();
        }
    }
}
