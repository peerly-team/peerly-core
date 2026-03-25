using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Groups.AddGroupParticipant;
using Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Groups;

[ExcludeFromCodeCoverage]
public sealed class GroupController : GroupService.GroupServiceBase
{
    private readonly ICommandHandler<CreateGroupCommand, CreateGroupCommandResponse> _createGroupHandler;
    private readonly ICommandHandler<AddGroupParticipantCommand, Success> _addGroupParticipantHandler;

    public GroupController(
        ICommandHandler<CreateGroupCommand, CreateGroupCommandResponse> createGroupHandler,
        ICommandHandler<AddGroupParticipantCommand, Success> addGroupParticipantHandler)
    {
        _createGroupHandler = createGroupHandler;
        _addGroupParticipantHandler = addGroupParticipantHandler;
    }

    public override async Task<V1CreateGroupResponse> V1CreateGroup(V1CreateGroupRequest request, ServerCallContext context)
    {
        var command = request.ToCreateGroupCommand();
        var commandResponse = await _createGroupHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1CreateGroupResponse();
    }

    public override async Task<V1AddGroupParticipantResponse> V1AddGroupParticipant(V1AddGroupParticipantRequest request, ServerCallContext context)
    {
        var command = request.ToAddGroupParticipantCommand();
        var commandResponse = await _addGroupParticipantHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1AddGroupParticipantResponse();
    }
}
