using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Groups.UpdateGroup.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.UpdateGroup;

internal sealed class UpdateGroupHandler : ICommandHandler<UpdateGroupCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IUpdateGroupValidator _validator;

    public UpdateGroupHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IUpdateGroupValidator validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        UpdateGroupCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var validationError = await _validator.ValidateAsync(unitOfWork, command, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        _ = await unitOfWork.GroupRepository.UpdateAsync(
            command.GroupId,
            builder => builder.Set(item => item.Name, command.Name),
            cancellationToken);

        return new Success();
    }
}
