using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.ListTeacherRubrics;

internal sealed class ListTeacherRubricsHandler : IQueryHandler<ListTeacherRubricsQuery, ListTeacherRubricsQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public ListTeacherRubricsHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<ListTeacherRubricsQueryResponse> ExecuteAsync(ListTeacherRubricsQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var rubrics = await unitOfWork.ReadOnlyRubricRepository.ListByTeacherAsync(query.TeacherId, cancellationToken);

        return new ListTeacherRubricsQueryResponse { Rubrics = rubrics };
    }
}
