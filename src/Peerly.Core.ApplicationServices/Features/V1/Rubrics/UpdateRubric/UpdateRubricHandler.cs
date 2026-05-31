using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.UpdateRubric;

internal sealed class UpdateRubricHandler : ICommandHandler<UpdateRubricCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICommandValidator<UpdateRubricCommand, Success> _validator;
    private readonly IClock _clock;

    public UpdateRubricHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ICommandValidator<UpdateRubricCommand, Success> validator,
        IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
        _clock = clock;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        UpdateRubricCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);
        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        await unitOfWork.RubricRepository.UpdateAsync(
            command.RubricId,
            builder => builder.Set(item => item.Name, command.Name),
            cancellationToken);

        await unitOfWork.RubricCriterionRepository.DeleteByRubricIdAsync(command.RubricId, cancellationToken);

        var criteriaAddItems = command.Criteria.ToRubricCriterionAddItems(command.RubricId,  _clock.GetCurrentTime());
        await unitOfWork.RubricCriterionRepository.BatchAddAsync(criteriaAddItems, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new Success();
    }
}
