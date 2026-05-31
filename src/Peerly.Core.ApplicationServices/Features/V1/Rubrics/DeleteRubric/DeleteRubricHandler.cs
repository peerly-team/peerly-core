using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.DeleteRubric;

internal sealed class DeleteRubricHandler : ICommandHandler<DeleteRubricCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICommandValidator<DeleteRubricCommand, Success> _validator;

    public DeleteRubricHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ICommandValidator<DeleteRubricCommand, Success> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        DeleteRubricCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);
        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        await unitOfWork.RubricCriterionRepository.DeleteByRubricIdAsync(command.RubricId, cancellationToken);
        await unitOfWork.RubricRepository.DeleteAsync(command.RubricId, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new Success();
    }
}
