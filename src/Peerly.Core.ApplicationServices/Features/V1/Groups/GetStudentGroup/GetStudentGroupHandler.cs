using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;

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

        var groupStudent = query.ToGroupStudent();
        if (!await unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(groupStudent, cancellationToken))
        {
            throw new NotFoundException();
        }

        return new GetStudentGroupQueryResponse
        {
            Group = group
        };
    }
}
