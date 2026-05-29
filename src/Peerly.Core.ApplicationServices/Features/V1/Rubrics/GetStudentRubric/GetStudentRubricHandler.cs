using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetStudentRubric;

internal sealed class GetStudentRubricHandler : IQueryHandler<GetStudentRubricQuery, GetStudentRubricQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IQueryValidator<GetStudentRubricQuery, GetStudentRubricQueryResponse> _validator;

    public GetStudentRubricHandler(
        ICommonUnitOfWorkFactory unitOfWorkFactory,
        IQueryValidator<GetStudentRubricQuery, GetStudentRubricQueryResponse> validator)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _validator = validator;
    }

    public async Task<GetStudentRubricQueryResponse> ExecuteAsync(GetStudentRubricQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(query, cancellationToken);

        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);
        var criteria = await unitOfWork.ReadOnlyRubricCriterionRepository.ListByRubricIdAsync(query.RubricId, cancellationToken);

        return new GetStudentRubricQueryResponse
        {
            Criteria = criteria
        };
    }
}
