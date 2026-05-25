using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;

namespace Peerly.Core.ApplicationServices.Features.V1.Students.GetStudent;

internal sealed class GetStudentHandler : IQueryHandler<GetStudentQuery, GetStudentQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetStudentHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetStudentQueryResponse> ExecuteAsync(GetStudentQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var student = await unitOfWork.ReadOnlyStudentRepository.GetAsync(query.StudentId, cancellationToken)
                      ?? throw new NotFoundException();

        return new GetStudentQueryResponse
        {
            Student = student
        };
    }
}
