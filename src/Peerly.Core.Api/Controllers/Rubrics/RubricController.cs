using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.CreateRubric;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.DeleteRubric;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetStudentRubric;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetTeacherRubric;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.ListTeacherRubrics;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.UpdateRubric;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Rubrics;

[ExcludeFromCodeCoverage]
public sealed class RubricController : RubricService.RubricServiceBase
{
    private readonly ICommandHandler<CreateRubricCommand, CreateRubricCommandResponse> _createRubricHandler;
    private readonly ICommandHandler<UpdateRubricCommand, Success> _updateRubricHandler;
    private readonly ICommandHandler<DeleteRubricCommand, Success> _deleteRubricHandler;
    private readonly IQueryHandler<GetTeacherRubricQuery, GetTeacherRubricQueryResponse> _getTeacherRubricHandler;
    private readonly IQueryHandler<GetStudentRubricQuery, GetStudentRubricQueryResponse> _getStudentRubricHandler;
    private readonly IQueryHandler<ListTeacherRubricsQuery, ListTeacherRubricsQueryResponse> _listTeacherRubricsHandler;

    public RubricController(
        ICommandHandler<CreateRubricCommand, CreateRubricCommandResponse> createRubricHandler,
        ICommandHandler<UpdateRubricCommand, Success> updateRubricHandler,
        ICommandHandler<DeleteRubricCommand, Success> deleteRubricHandler,
        IQueryHandler<GetTeacherRubricQuery, GetTeacherRubricQueryResponse> getTeacherRubricHandler,
        IQueryHandler<GetStudentRubricQuery, GetStudentRubricQueryResponse> getStudentRubricHandler,
        IQueryHandler<ListTeacherRubricsQuery, ListTeacherRubricsQueryResponse> listTeacherRubricsHandler)
    {
        _createRubricHandler = createRubricHandler;
        _updateRubricHandler = updateRubricHandler;
        _deleteRubricHandler = deleteRubricHandler;
        _getTeacherRubricHandler = getTeacherRubricHandler;
        _getStudentRubricHandler = getStudentRubricHandler;
        _listTeacherRubricsHandler = listTeacherRubricsHandler;
    }

    public override async Task<V1CreateRubricResponse> V1CreateRubric(V1CreateRubricRequest request, ServerCallContext context)
    {
        var command = request.ToCreateRubricCommand();
        var commandResponse = await _createRubricHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1CreateRubricResponse();
    }

    public override async Task<V1UpdateRubricResponse> V1UpdateRubric(V1UpdateRubricRequest request, ServerCallContext context)
    {
        var command = request.ToUpdateRubricCommand();
        var commandResponse = await _updateRubricHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1UpdateRubricResponse();
    }

    public override async Task<V1DeleteRubricResponse> V1DeleteRubric(V1DeleteRubricRequest request, ServerCallContext context)
    {
        var command = request.ToDeleteRubricCommand();
        var commandResponse = await _deleteRubricHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1DeleteRubricResponse();
    }

    public override async Task<V1GetTeacherRubricResponse> V1GetTeacherRubric(V1GetTeacherRubricRequest request, ServerCallContext context)
    {
        var query = request.ToGetTeacherRubricQuery();
        var queryResponse = await _getTeacherRubricHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetTeacherRubricResponse();
    }

    public override async Task<V1GetStudentRubricResponse> V1GetStudentRubric(V1GetStudentRubricRequest request, ServerCallContext context)
    {
        var query = request.ToGetStudentRubricQuery();
        var queryResponse = await _getStudentRubricHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetStudentRubricResponse();
    }

    public override async Task<V1ListTeacherRubricsResponse> V1ListTeacherRubrics(V1ListTeacherRubricsRequest request, ServerCallContext context)
    {
        var query = request.ToListTeacherRubricsQuery();
        var queryResponse = await _listTeacherRubricsHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1ListTeacherRubricsResponse();
    }
}
