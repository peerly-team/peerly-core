using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Groups.AddGroupParticipant;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Participants;

[ExcludeFromCodeCoverage]
public sealed class ParticipantController : ParticipantService.ParticipantServiceBase
{
    private readonly ICommandHandler<AddGroupParticipantCommand, Success> _addGroupParticipantHandler;

    public ParticipantController(ICommandHandler<AddGroupParticipantCommand, Success> addGroupParticipantHandler)
    {
        _addGroupParticipantHandler = addGroupParticipantHandler;
    }

    public override async Task<V1AddGroupParticipantResponse> V1AddGroupParticipant(V1AddGroupParticipantRequest request, ServerCallContext context)
    {
        var command = request.ToAddGroupParticipantCommand();
        var commandResponse = await _addGroupParticipantHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1AddGroupParticipantResponse();
    }
}
