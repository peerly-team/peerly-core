using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;

namespace Peerly.Core.ApplicationServices.Features.V1.Teachers.GetTeacher;

internal sealed class GetTeacherHandler : IQueryHandler<GetTeacherQuery, GetTeacherQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetTeacherHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetTeacherQueryResponse> ExecuteAsync(GetTeacherQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var teacher = await unitOfWork.ReadOnlyTeacherRepository.GetAsync(query.TeacherId, cancellationToken)
                      ?? throw new NotFoundException();

        return new GetTeacherQueryResponse
        {
            Teacher = teacher
        };
    }
}
