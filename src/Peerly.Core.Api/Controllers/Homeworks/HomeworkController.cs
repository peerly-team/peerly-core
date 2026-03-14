using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchStudentCourseHomeworks;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateHomeworkStatus;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Homeworks;

[ExcludeFromCodeCoverage]
public sealed class HomeworkController : HomeworkService.HomeworkServiceBase
{
    private readonly IQueryHandler<SearchStudentCourseHomeworksQuery, SearchStudentCourseHomeworksQueryResponse> _searchStudentCourseHomeworksHandler;
    private readonly ICommandHandler<CreateHomeworkCommand, CreateHomeworkCommandResponse> _createHomeworkHandler;
    private readonly ICommandHandler<UpdateHomeworkStatusCommand, Success> _updateHomeworkStatusHandler;
    private readonly ICommandHandler<CreateHomeworkFileCommand, CreateHomeworkFileCommandResponse> _createHomeworkAttachmentHandler;

    public HomeworkController(
        IQueryHandler<SearchStudentCourseHomeworksQuery, SearchStudentCourseHomeworksQueryResponse> searchStudentCourseHomeworksHandler,
        ICommandHandler<CreateHomeworkCommand, CreateHomeworkCommandResponse> createHomeworkHandler,
        ICommandHandler<UpdateHomeworkStatusCommand, Success> updateHomeworkStatusHandler,
        ICommandHandler<CreateHomeworkFileCommand, CreateHomeworkFileCommandResponse> createHomeworkAttachmentHandler)
    {
        _searchStudentCourseHomeworksHandler = searchStudentCourseHomeworksHandler;
        _createHomeworkHandler = createHomeworkHandler;
        _updateHomeworkStatusHandler = updateHomeworkStatusHandler;
        _createHomeworkAttachmentHandler = createHomeworkAttachmentHandler;
    }

    public override async Task<V1CreateHomeworkResponse> V1CreateHomework(V1CreateHomeworkRequest request, ServerCallContext context)
    {
        var command = request.ToCreateHomeworkCommand();
        var commandResponse = await _createHomeworkHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1CreateHomeworkResponse();
    }

    public override async Task<V1UpdateHomeworkStatusResponse> V1UpdateHomeworkStatus(V1UpdateHomeworkStatusRequest request, ServerCallContext context)
    {
        var command = request.ToUpdateHomeworkStatusCommand();
        var commandResponse = await _updateHomeworkStatusHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1UpdateHomeworkStatusResponse();
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
