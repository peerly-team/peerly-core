using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Groups.AddGroupParticipant;
using Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup;
using Peerly.Core.ApplicationServices.Features.V1.Groups.SearchCourseGroups;
using Peerly.Core.ApplicationServices.Features.V1.Groups.SearchGroupStudents;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Groups;

[ExcludeFromCodeCoverage]
public sealed class GroupController : GroupService.GroupServiceBase
{
    private readonly ICommandHandler<CreateGroupCommand, CreateGroupCommandResponse> _createGroupHandler;
    private readonly ICommandHandler<AddGroupParticipantCommand, Success> _addGroupParticipantHandler;
    private readonly IQueryHandler<SearchCourseGroupsQuery, SearchCourseGroupsQueryResponse> _searchCourseGroupsHandler;
    private readonly IQueryHandler<SearchGroupStudentsQuery, SearchGroupStudentsQueryResponse> _searchGroupStudentsHandler;

    public GroupController(
        ICommandHandler<CreateGroupCommand, CreateGroupCommandResponse> createGroupHandler,
        ICommandHandler<AddGroupParticipantCommand, Success> addGroupParticipantHandler,
        IQueryHandler<SearchCourseGroupsQuery, SearchCourseGroupsQueryResponse> searchCourseGroupsHandler,
        IQueryHandler<SearchGroupStudentsQuery, SearchGroupStudentsQueryResponse> searchGroupStudentsHandler)
    {
        _createGroupHandler = createGroupHandler;
        _addGroupParticipantHandler = addGroupParticipantHandler;
        _searchCourseGroupsHandler = searchCourseGroupsHandler;
        _searchGroupStudentsHandler = searchGroupStudentsHandler;
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

    public override async Task<V1SearchCourseGroupsResponse> V1SearchCourseGroups(V1SearchCourseGroupsRequest request, ServerCallContext context)
    {
        var query = request.ToSearchCourseGroupsQuery();
        var queryResponse = await _searchCourseGroupsHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1SearchCourseGroupsResponse();
    }

    public override async Task<V1SearchGroupStudentsResponse> V1SearchGroupStudents(V1SearchGroupStudentsRequest request, ServerCallContext context)
    {
        var query = request.ToSearchGroupStudentsQuery();
        var queryResponse = await _searchGroupStudentsHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1SearchGroupStudentsResponse();
    }
}
