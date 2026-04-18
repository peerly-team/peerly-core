using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Groups.AddGroupParticipant;
using Peerly.Core.ApplicationServices.Features.V1.Participants.ListCourseParticipants;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Participants;

[ExcludeFromCodeCoverage]
public sealed class ParticipantController : ParticipantService.ParticipantServiceBase
{
    private readonly IQueryHandler<ListCourseParticipantsQuery, ListCourseParticipantsQueryResponse> _listCourseParticipantsHandler;
    private readonly ICommandHandler<AddGroupParticipantCommand, Success> _addGroupParticipantHandler;

    public ParticipantController(
        IQueryHandler<ListCourseParticipantsQuery, ListCourseParticipantsQueryResponse> listCourseParticipantsHandler,
        ICommandHandler<AddGroupParticipantCommand, Success> addGroupParticipantHandler)
    {
        _listCourseParticipantsHandler = listCourseParticipantsHandler;
        _addGroupParticipantHandler = addGroupParticipantHandler;
    }

    public override async Task<V1ListCourseParticipantsResponse> V1ListCourseParticipants(V1ListCourseParticipantsRequest request, ServerCallContext context)
    {
        var query = request.ToListCourseParticipantsQuery();
        var queryResponse = await _listCourseParticipantsHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1ListCourseParticipantsResponse();
    }

    public override async Task<V1AddGroupParticipantResponse> V1AddGroupParticipant(V1AddGroupParticipantRequest request, ServerCallContext context)
    {
        var command = request.ToAddGroupParticipantCommand();
        var commandResponse = await _addGroupParticipantHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1AddGroupParticipantResponse();
    }
}
