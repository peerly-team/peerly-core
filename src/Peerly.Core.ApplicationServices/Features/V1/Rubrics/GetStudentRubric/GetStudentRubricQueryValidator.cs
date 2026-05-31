using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetStudentRubric;

// TODO: добавить проверку участия студента (назначен на review с домашкой, ссылающейся на эту рубрику)
internal sealed class GetStudentRubricQueryValidator : IQueryValidator<GetStudentRubricQuery, GetStudentRubricQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public GetStudentRubricQueryValidator(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task ValidateAsync(GetStudentRubricQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        _ = await unitOfWork.ReadOnlyRubricRepository.GetAsync(query.RubricId, cancellationToken)
            ?? throw new NotFoundException();
    }
}
