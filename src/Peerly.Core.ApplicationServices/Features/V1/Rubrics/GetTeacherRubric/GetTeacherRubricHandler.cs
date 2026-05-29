using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetTeacherRubric;

internal sealed class GetTeacherRubricHandler : IQueryHandler<GetTeacherRubricQuery, GetTeacherRubricQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetTeacherRubricHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetTeacherRubricQueryResponse> ExecuteAsync(GetTeacherRubricQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var rubric = await unitOfWork.ReadOnlyRubricRepository.GetAsync(query.RubricId, cancellationToken);
        if (rubric is null || rubric.TeacherId != query.TeacherId)
        {
            throw new NotFoundException();
        }

        var criteria = await unitOfWork.ReadOnlyRubricCriterionRepository.ListByRubricIdAsync(query.RubricId, cancellationToken);

        return new GetTeacherRubricQueryResponse
        {
            Rubric = rubric,
            Criteria = criteria
        };
    }
}
