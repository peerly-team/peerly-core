using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Students.GetStudent;
using Peerly.Core.ApplicationServices.Features.V1.Students.UpdateStudent;
using Peerly.Core.ApplicationServices.Features.V1.Teachers.GetTeacher;
using Peerly.Core.ApplicationServices.Features.V1.Teachers.UpdateTeacher;
using Peerly.Core.ApplicationServices.Features.V1.Users.SearchUsers;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Users;

[ExcludeFromCodeCoverage]
public sealed class UserController : UserService.UserServiceBase
{
    private readonly IQueryHandler<SearchUsersQuery, SearchUsersQueryResponse> _searchUsersHandler;
    private readonly IQueryHandler<GetStudentQuery, GetStudentQueryResponse> _getStudentHandler;
    private readonly IQueryHandler<GetTeacherQuery, GetTeacherQueryResponse> _getTeacherHandler;
    private readonly ICommandHandler<UpdateStudentCommand, Success> _updateStudentHandler;
    private readonly ICommandHandler<UpdateTeacherCommand, Success> _updateTeacherHandler;

    public UserController(
        IQueryHandler<SearchUsersQuery, SearchUsersQueryResponse> searchUsersHandler,
        IQueryHandler<GetStudentQuery, GetStudentQueryResponse> getStudentHandler,
        IQueryHandler<GetTeacherQuery, GetTeacherQueryResponse> getTeacherHandler,
        ICommandHandler<UpdateStudentCommand, Success> updateStudentHandler,
        ICommandHandler<UpdateTeacherCommand, Success> updateTeacherHandler)
    {
        _searchUsersHandler = searchUsersHandler;
        _getStudentHandler = getStudentHandler;
        _getTeacherHandler = getTeacherHandler;
        _updateStudentHandler = updateStudentHandler;
        _updateTeacherHandler = updateTeacherHandler;
    }

    public override async Task<V1SearchUsersResponse> V1SearchUsers(V1SearchUsersRequest request, ServerCallContext context)
    {
        var query = request.ToSearchUsersQuery();
        var queryResponse = await _searchUsersHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1SearchUsersResponse();
    }

    public override async Task<V1GetStudentResponse> V1GetStudent(V1GetStudentRequest request, ServerCallContext context)
    {
        var query = request.ToGetStudentQuery();
        var queryResponse = await _getStudentHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetStudentResponse();
    }

    public override async Task<V1GetTeacherResponse> V1GetTeacher(V1GetTeacherRequest request, ServerCallContext context)
    {
        var query = request.ToGetTeacherQuery();
        var queryResponse = await _getTeacherHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetTeacherResponse();
    }

    public override async Task<V1UpdateStudentResponse> V1UpdateStudent(V1UpdateStudentRequest request, ServerCallContext context)
    {
        var command = request.ToUpdateStudentCommand();
        var commandResponse = await _updateStudentHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1UpdateStudentResponse();
    }

    public override async Task<V1UpdateTeacherResponse> V1UpdateTeacher(V1UpdateTeacherRequest request, ServerCallContext context)
    {
        var command = request.ToUpdateTeacherCommand();
        var commandResponse = await _updateTeacherHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1UpdateTeacherResponse();
    }
}
