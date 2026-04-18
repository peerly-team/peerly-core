using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupStudent;
using Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupTeacher;
using Peerly.Core.ApplicationServices.Features.V1.Participants.ListCourseParticipants;
using Peerly.Core.ApplicationServices.Features.V1.Participants.ListGroupParticipants;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Participants;

[ExcludeFromCodeCoverage]
public sealed class ParticipantController : ParticipantService.ParticipantServiceBase
{
    private readonly IQueryHandler<ListCourseParticipantsQuery, ListCourseParticipantsQueryResponse> _listCourseParticipantsHandler;
    private readonly IQueryHandler<ListGroupParticipantsQuery, ListGroupParticipantsQueryResponse> _listGroupParticipantsHandler;
    private readonly ICommandHandler<AddGroupStudentCommand, Success> _addGroupStudentHandler;
    private readonly ICommandHandler<AddGroupTeacherCommand, Success> _addGroupTeacherHandler;

    public ParticipantController(
        IQueryHandler<ListCourseParticipantsQuery, ListCourseParticipantsQueryResponse> listCourseParticipantsHandler,
        IQueryHandler<ListGroupParticipantsQuery, ListGroupParticipantsQueryResponse> listGroupParticipantsHandler,
        ICommandHandler<AddGroupStudentCommand, Success> addGroupStudentHandler,
        ICommandHandler<AddGroupTeacherCommand, Success> addGroupTeacherHandler)
    {
        _listCourseParticipantsHandler = listCourseParticipantsHandler;
        _listGroupParticipantsHandler = listGroupParticipantsHandler;
        _addGroupStudentHandler = addGroupStudentHandler;
        _addGroupTeacherHandler = addGroupTeacherHandler;
    }

    public override async Task<V1ListCourseParticipantsResponse> V1ListCourseParticipants(V1ListCourseParticipantsRequest request, ServerCallContext context)
    {
        var query = request.ToListCourseParticipantsQuery();
        var queryResponse = await _listCourseParticipantsHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1ListCourseParticipantsResponse();
    }

    public override async Task<V1ListGroupParticipantsResponse> V1ListGroupParticipants(V1ListGroupParticipantsRequest request, ServerCallContext context)
    {
        var query = request.ToListGroupParticipantsQuery();
        var queryResponse = await _listGroupParticipantsHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1ListGroupParticipantsResponse();
    }

    public override async Task<V1AddGroupStudentResponse> V1AddGroupStudent(V1AddGroupStudentRequest request, ServerCallContext context)
    {
        var command = request.ToAddGroupStudentCommand();
        var commandResponse = await _addGroupStudentHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1AddGroupStudentResponse();
    }

    public override async Task<V1AddGroupTeacherResponse> V1AddGroupTeacher(V1AddGroupTeacherRequest request, ServerCallContext context)
    {
        var command = request.ToAddGroupTeacherCommand();
        var commandResponse = await _addGroupTeacherHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1AddGroupTeacherResponse();
    }
}
