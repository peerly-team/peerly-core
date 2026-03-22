using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Groups;

[ExcludeFromCodeCoverage]
public sealed class GroupController : GroupService.GroupServiceBase
{
    private readonly ICommandHandler<CreateGroupCommand, CreateGroupCommandResponse> _createGroupHandler;

    public GroupController(ICommandHandler<CreateGroupCommand, CreateGroupCommandResponse> createGroupHandler)
    {
        _createGroupHandler = createGroupHandler;
    }

    public override async Task<V1CreateGroupResponse> V1CreateGroup(V1CreateGroupRequest request, ServerCallContext context)
    {
        var command = request.ToCreateGroupCommand();
        var commandResponse = await _createGroupHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1CreateGroupResponse();
    }
}
