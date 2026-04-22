using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Groups.DeleteGroup.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.DeleteGroup;

internal sealed class DeleteGroupHandler : ICommandHandler<DeleteGroupCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IDeleteGroupValidator _validator;

    public DeleteGroupHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IDeleteGroupValidator validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        DeleteGroupCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var validationError = await _validator.ValidateAsync(unitOfWork, command, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        await unitOfWork.GroupStudentRepository.DeleteByGroupAsync(command.GroupId, cancellationToken);
        await unitOfWork.GroupTeacherRepository.DeleteByGroupAsync(command.GroupId, cancellationToken);
        await unitOfWork.GroupRepository.DeleteAsync(command.GroupId, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new Success();
    }
}
