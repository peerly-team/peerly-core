using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup;
using Peerly.Core.ApplicationServices.Features.V1.Groups.DeleteGroup;
using Peerly.Core.ApplicationServices.Features.V1.Groups.GetStudentGroup;
using Peerly.Core.ApplicationServices.Features.V1.Groups.GetTeacherGroup;
using Peerly.Core.ApplicationServices.Features.V1.Groups.ListStudentCourseGroups;
using Peerly.Core.ApplicationServices.Features.V1.Groups.ListTeacherCourseGroups;
using Peerly.Core.ApplicationServices.Features.V1.Groups.UpdateGroup;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Groups;

[ExcludeFromCodeCoverage]
public sealed class GroupController : GroupService.GroupServiceBase
{
    private readonly ICommandHandler<CreateGroupCommand, CreateGroupCommandResponse> _createGroupHandler;
    private readonly ICommandHandler<UpdateGroupCommand, Success> _updateGroupHandler;
    private readonly ICommandHandler<DeleteGroupCommand, Success> _deleteGroupHandler;
    private readonly IQueryHandler<GetTeacherGroupQuery, GetTeacherGroupQueryResponse> _getTeacherGroupHandler;
    private readonly IQueryHandler<GetStudentGroupQuery, GetStudentGroupQueryResponse> _getStudentGroupHandler;
    private readonly IQueryHandler<ListTeacherCourseGroupsQuery, ListTeacherCourseGroupsQueryResponse> _listTeacherCourseGroupsHandler;
    private readonly IQueryHandler<ListStudentCourseGroupsQuery, ListStudentCourseGroupsQueryResponse> _listStudentCourseGroupsHandler;

    public GroupController(
        ICommandHandler<CreateGroupCommand, CreateGroupCommandResponse> createGroupHandler,
        ICommandHandler<UpdateGroupCommand, Success> updateGroupHandler,
        ICommandHandler<DeleteGroupCommand, Success> deleteGroupHandler,
        IQueryHandler<GetTeacherGroupQuery, GetTeacherGroupQueryResponse> getTeacherGroupHandler,
        IQueryHandler<GetStudentGroupQuery, GetStudentGroupQueryResponse> getStudentGroupHandler,
        IQueryHandler<ListTeacherCourseGroupsQuery, ListTeacherCourseGroupsQueryResponse> listTeacherCourseGroupsHandler,
        IQueryHandler<ListStudentCourseGroupsQuery, ListStudentCourseGroupsQueryResponse> listStudentCourseGroupsHandler)
    {
        _createGroupHandler = createGroupHandler;
        _updateGroupHandler = updateGroupHandler;
        _deleteGroupHandler = deleteGroupHandler;
        _getTeacherGroupHandler = getTeacherGroupHandler;
        _getStudentGroupHandler = getStudentGroupHandler;
        _listTeacherCourseGroupsHandler = listTeacherCourseGroupsHandler;
        _listStudentCourseGroupsHandler = listStudentCourseGroupsHandler;
    }

    public override async Task<V1CreateGroupResponse> V1CreateGroup(V1CreateGroupRequest request, ServerCallContext context)
    {
        var command = request.ToCreateGroupCommand();
        var commandResponse = await _createGroupHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1CreateGroupResponse();
    }

    public override async Task<V1UpdateGroupResponse> V1UpdateGroup(V1UpdateGroupRequest request, ServerCallContext context)
    {
        var command = request.ToUpdateGroupCommand();
        var commandResponse = await _updateGroupHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1UpdateGroupResponse();
    }

    public override async Task<V1DeleteGroupResponse> V1DeleteGroup(V1DeleteGroupRequest request, ServerCallContext context)
    {
        var command = request.ToDeleteGroupCommand();
        var commandResponse = await _deleteGroupHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1DeleteGroupResponse();
    }

    public override async Task<V1GetTeacherGroupResponse> V1GetTeacherGroup(V1GetTeacherGroupRequest request, ServerCallContext context)
    {
        var query = request.ToGetTeacherGroupQuery();
        var queryResponse = await _getTeacherGroupHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetTeacherGroupResponse();
    }

    public override async Task<V1GetStudentGroupResponse> V1GetStudentGroup(V1GetStudentGroupRequest request, ServerCallContext context)
    {
        var query = request.ToGetStudentGroupQuery();
        var queryResponse = await _getStudentGroupHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetStudentGroupResponse();
    }

    public override async Task<V1ListTeacherCourseGroupsResponse> V1ListTeacherCourseGroups(V1ListTeacherCourseGroupsRequest request, ServerCallContext context)
    {
        var query = request.ToListTeacherCourseGroupsQuery();
        var queryResponse = await _listTeacherCourseGroupsHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1ListTeacherCourseGroupsResponse();
    }

    public override async Task<V1ListStudentCourseGroupsResponse> V1ListStudentCourseGroups(V1ListStudentCourseGroupsRequest request, ServerCallContext context)
    {
        var query = request.ToListStudentCourseGroupsQuery();
        var queryResponse = await _listStudentCourseGroupsHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1ListStudentCourseGroupsResponse();
    }
}
