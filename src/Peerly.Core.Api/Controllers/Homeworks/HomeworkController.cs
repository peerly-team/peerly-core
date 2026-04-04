using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchStudentCourseHomeworks;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateHomeworkStatus;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Homeworks;

[ExcludeFromCodeCoverage]
public sealed class HomeworkController : HomeworkService.HomeworkServiceBase
{
    private readonly IQueryHandler<SearchStudentCourseHomeworksQuery, SearchStudentCourseHomeworksQueryResponse> _searchStudentCourseHomeworksHandler;
    private readonly ICommandHandler<CreateCourseHomeworkCommand, CreateCourseHomeworkCommandResponse> _createHomeworkHandler;
    private readonly ICommandHandler<CreateGroupHomeworkCommand, CreateGroupHomeworkCommandResponse> _createGroupHomeworkHandler;
    private readonly ICommandHandler<UpdateHomeworkStatusCommand, Success> _updateHomeworkStatusHandler;
    private readonly ICommandHandler<ConfirmHomeworkCommand, Success> _confirmHomeworkHandler;
    private readonly ICommandHandler<CreateHomeworkFileCommand, CreateHomeworkFileCommandResponse> _createHomeworkAttachmentHandler;

    public HomeworkController(
        IQueryHandler<SearchStudentCourseHomeworksQuery, SearchStudentCourseHomeworksQueryResponse> searchStudentCourseHomeworksHandler,
        ICommandHandler<CreateCourseHomeworkCommand, CreateCourseHomeworkCommandResponse> createHomeworkHandler,
        ICommandHandler<CreateGroupHomeworkCommand, CreateGroupHomeworkCommandResponse> createGroupHomeworkHandler,
        ICommandHandler<UpdateHomeworkStatusCommand, Success> updateHomeworkStatusHandler,
        ICommandHandler<ConfirmHomeworkCommand, Success> confirmHomeworkHandler,
        ICommandHandler<CreateHomeworkFileCommand, CreateHomeworkFileCommandResponse> createHomeworkAttachmentHandler)
    {
        _searchStudentCourseHomeworksHandler = searchStudentCourseHomeworksHandler;
        _createHomeworkHandler = createHomeworkHandler;
        _createGroupHomeworkHandler = createGroupHomeworkHandler;
        _updateHomeworkStatusHandler = updateHomeworkStatusHandler;
        _confirmHomeworkHandler = confirmHomeworkHandler;
        _createHomeworkAttachmentHandler = createHomeworkAttachmentHandler;
    }

    public override async Task<V1CreateCourseHomeworkResponse> V1CreateCourseHomework(V1CreateCourseHomeworkRequest request, ServerCallContext context)
    {
        var command = request.ToCreateCourseHomeworkCommand();
        var commandResponse = await _createHomeworkHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1CreateCourseHomeworkResponse();
    }

    public override async Task<V1CreateGroupHomeworkResponse> V1CreateGroupHomework(V1CreateGroupHomeworkRequest request, ServerCallContext context)
    {
        var command = request.ToCreateGroupHomeworkCommand();
        var commandResponse = await _createGroupHomeworkHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1CreateGroupHomeworkResponse();
    }

    public override async Task<V1UpdateHomeworkStatusResponse> V1UpdateHomeworkStatus(V1UpdateHomeworkStatusRequest request, ServerCallContext context)
    {
        var command = request.ToUpdateHomeworkStatusCommand();
        var commandResponse = await _updateHomeworkStatusHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1UpdateHomeworkStatusResponse();
    }

    public override async Task<V1ConfirmHomeworkResponse> V1ConfirmHomework(V1ConfirmHomeworkRequest request, ServerCallContext context)
    {
        var command = request.ToConfirmHomeworkCommand();
        var commandResponse = await _confirmHomeworkHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1ConfirmHomeworkResponse();
    }

    public override async Task<V1CreateHomeworkFileResponse> V1CreateHomeworkFile(V1CreateHomeworkFileRequest request, ServerCallContext context)
    {
        var command = request.ToCreateHomeworkFileCommand();
        var commandResponse = await _createHomeworkAttachmentHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1CreateHomeworkAttachmentResponse();
    }

    // todo: поправить ручку
    public override async Task<V1SearchStudentCourseHomeworksResponse> V1SearchStudentCourseHomeworks(
        V1SearchStudentCourseHomeworksRequest request,
        ServerCallContext context)
    {
        var query = request.ToSearchStudentCoursesQuery();
        var queryResponse = await _searchStudentCourseHomeworksHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1SearchStudentCoursesResponse();
    }
}
