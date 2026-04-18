using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;

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

        var groupTeacher = query.ToGroupTeacher();
        if (!await unitOfWork.ReadOnlyGroupTeacherRepository.ExistsAsync(groupTeacher, cancellationToken))
        {
            throw new NotFoundException();
        }

        var group = await unitOfWork.ReadOnlyGroupRepository.GetAsync(query.GroupId, cancellationToken)
                    ?? throw new NotFoundException();

        return new GetTeacherGroupQueryResponse
        {
            Group = group
        };
    }
}
