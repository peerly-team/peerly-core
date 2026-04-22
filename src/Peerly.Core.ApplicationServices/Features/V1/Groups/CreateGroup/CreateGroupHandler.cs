using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup;

internal sealed class CreateGroupHandler : ICommandHandler<CreateGroupCommand, CreateGroupCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICreateGroupValidator _validator;
    private readonly IClock _clock;

    public CreateGroupHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ICreateGroupValidator validator,
        IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
        _clock = clock;
    }

    public async Task<CommandResponse<CreateGroupCommandResponse>> ExecuteAsync(
        CreateGroupCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var validationError = await _validator.ValidateAsync(unitOfWork, command, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var groupAddItem = command.ToGroupAddItem(_clock.GetCurrentTime());
        var groupId = await unitOfWork.GroupRepository.AddAsync(groupAddItem, cancellationToken);

        return new CreateGroupCommandResponse
        {
            GroupId = groupId
        };
    }
}
