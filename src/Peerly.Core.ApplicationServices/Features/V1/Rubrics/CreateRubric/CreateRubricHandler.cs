using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.CreateRubric;

internal sealed class CreateRubricHandler : ICommandHandler<CreateRubricCommand, CreateRubricCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IClock _clock;

    public CreateRubricHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory, IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandResponse<CreateRubricCommandResponse>> ExecuteAsync(
        CreateRubricCommand command,
        CancellationToken cancellationToken)
    {
        var creationTime = _clock.GetCurrentTime();

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);
        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        var rubricAddItem = command.ToRubricAddItem(creationTime);
        var rubricId = await unitOfWork.RubricRepository.AddAsync(rubricAddItem, cancellationToken);

        var criteriaAddItems = command.Criteria.ToRubricCriterionAddItems(rubricId, creationTime);
        await unitOfWork.RubricCriterionRepository.BatchAddAsync(criteriaAddItems, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new CreateRubricCommandResponse { RubricId = rubricId };
    }
}
