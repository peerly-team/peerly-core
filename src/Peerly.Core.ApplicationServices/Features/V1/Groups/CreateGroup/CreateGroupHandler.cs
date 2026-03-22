using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup;

internal sealed class CreateGroupHandler : ICommandHandler<CreateGroupCommand, CreateGroupCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IClock _clock;

    public CreateGroupHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory, IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandResponse<CreateGroupCommandResponse>> ExecuteAsync(
        CreateGroupCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        // todo: добавить проверку, что курс существует
        // todo: добавить проверку, что препод может создавать группу на курсе

        var groupAddItem = new GroupAddItem
        {
            CourseId = command.CourseId,
            Name = command.Name,
            CreationTime = _clock.GetCurrentTime()
        };
        var groupId = await unitOfWork.GroupRepository.AddAsync(groupAddItem, cancellationToken);

        return new CreateGroupCommandResponse
        {
            GroupId = groupId
        };
    }
}
