using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PostponeHomeworkDeadlines;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PublishHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetStudentHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchStudentCourseHomeworks;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchTeacherCourseHomeworks;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateDraftHomework;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Homeworks;

[ExcludeFromCodeCoverage]
public sealed class HomeworkController : HomeworkService.HomeworkServiceBase
{
    private readonly IQueryHandler<SearchStudentCourseHomeworksQuery, SearchStudentCourseHomeworksQueryResponse> _searchStudentCourseHomeworksHandler;
    private readonly IQueryHandler<SearchTeacherCourseHomeworksQuery, SearchTeacherCourseHomeworksQueryResponse> _searchTeacherCourseHomeworksHandler;
    private readonly IQueryHandler<GetTeacherHomeworkQuery, GetTeacherHomeworkQueryResponse> _getTeacherHomeworkHandler;
    private readonly IQueryHandler<GetStudentHomeworkQuery, GetStudentHomeworkQueryResponse> _getStudentHomeworkHandler;
    private readonly ICommandHandler<CreateCourseHomeworkCommand, CreateCourseHomeworkCommandResponse> _createHomeworkHandler;
    private readonly ICommandHandler<CreateGroupHomeworkCommand, CreateGroupHomeworkCommandResponse> _createGroupHomeworkHandler;
    private readonly ICommandHandler<PublishHomeworkCommand, Success> _publishHomeworkHandler;
    private readonly ICommandHandler<ConfirmHomeworkCommand, Success> _confirmHomeworkHandler;
    private readonly ICommandHandler<CreateHomeworkFileCommand, CreateHomeworkFileCommandResponse> _createHomeworkAttachmentHandler;
    private readonly ICommandHandler<UpdateDraftHomeworkCommand, Success> _updateDraftHomeworkHandler;
    private readonly ICommandHandler<PostponeHomeworkDeadlinesCommand, Success> _postponeHomeworkDeadlinesHandler;

    public HomeworkController(
        IQueryHandler<SearchStudentCourseHomeworksQuery, SearchStudentCourseHomeworksQueryResponse> searchStudentCourseHomeworksHandler,
        IQueryHandler<SearchTeacherCourseHomeworksQuery, SearchTeacherCourseHomeworksQueryResponse> searchTeacherCourseHomeworksHandler,
        IQueryHandler<GetTeacherHomeworkQuery, GetTeacherHomeworkQueryResponse> getTeacherHomeworkHandler,
        IQueryHandler<GetStudentHomeworkQuery, GetStudentHomeworkQueryResponse> getStudentHomeworkHandler,
        ICommandHandler<CreateCourseHomeworkCommand, CreateCourseHomeworkCommandResponse> createHomeworkHandler,
        ICommandHandler<CreateGroupHomeworkCommand, CreateGroupHomeworkCommandResponse> createGroupHomeworkHandler,
        ICommandHandler<PublishHomeworkCommand, Success> publishHomeworkHandler,
        ICommandHandler<ConfirmHomeworkCommand, Success> confirmHomeworkHandler,
        ICommandHandler<CreateHomeworkFileCommand, CreateHomeworkFileCommandResponse> createHomeworkAttachmentHandler,
        ICommandHandler<UpdateDraftHomeworkCommand, Success> updateDraftHomeworkHandler,
        ICommandHandler<PostponeHomeworkDeadlinesCommand, Success> postponeHomeworkDeadlinesHandler)
    {
        _searchStudentCourseHomeworksHandler = searchStudentCourseHomeworksHandler;
        _searchTeacherCourseHomeworksHandler = searchTeacherCourseHomeworksHandler;
        _getTeacherHomeworkHandler = getTeacherHomeworkHandler;
        _getStudentHomeworkHandler = getStudentHomeworkHandler;
        _createHomeworkHandler = createHomeworkHandler;
        _createGroupHomeworkHandler = createGroupHomeworkHandler;
        _publishHomeworkHandler = publishHomeworkHandler;
        _confirmHomeworkHandler = confirmHomeworkHandler;
        _createHomeworkAttachmentHandler = createHomeworkAttachmentHandler;
        _updateDraftHomeworkHandler = updateDraftHomeworkHandler;
        _postponeHomeworkDeadlinesHandler = postponeHomeworkDeadlinesHandler;
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

    public override async Task<V1PublishHomeworkResponse> V1PublishHomework(V1PublishHomeworkRequest request, ServerCallContext context)
    {
        var command = request.ToPublishHomeworkCommand();
        var commandResponse = await _publishHomeworkHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1PublishHomeworkResponse();
    }

    public override async Task<V1ConfirmHomeworkResponse> V1ConfirmHomework(V1ConfirmHomeworkRequest request, ServerCallContext context)
    {
        var command = request.ToConfirmHomeworkCommand();
        var commandResponse = await _confirmHomeworkHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1ConfirmHomeworkResponse();
    }

    public override async Task<V1UpdateDraftHomeworkResponse> V1UpdateDraftHomework(V1UpdateDraftHomeworkRequest request, ServerCallContext context)
    {
        var command = request.ToUpdateDraftHomeworkCommand();
        var commandResponse = await _updateDraftHomeworkHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1UpdateDraftHomeworkResponse();
    }

    public override async Task<V1PostponeHomeworkDeadlinesResponse> V1PostponeHomeworkDeadlines(V1PostponeHomeworkDeadlinesRequest request, ServerCallContext context)
    {
        var command = request.ToPostponeHomeworkDeadlinesCommand();
        var commandResponse = await _postponeHomeworkDeadlinesHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1PostponeHomeworkDeadlinesResponse();
    }

    public override async Task<V1CreateHomeworkFileResponse> V1CreateHomeworkFile(V1CreateHomeworkFileRequest request, ServerCallContext context)
    {
        var command = request.ToCreateHomeworkFileCommand();
        var commandResponse = await _createHomeworkAttachmentHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1CreateHomeworkAttachmentResponse();
    }

    public override async Task<V1GetTeacherHomeworkResponse> V1GetTeacherHomework(V1GetTeacherHomeworkRequest request, ServerCallContext context)
    {
        var query = request.ToGetTeacherHomeworkQuery();
        var queryResponse = await _getTeacherHomeworkHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetTeacherHomeworkResponse();
    }

    public override async Task<V1GetStudentHomeworkResponse> V1GetStudentHomework(V1GetStudentHomeworkRequest request, ServerCallContext context)
    {
        var query = request.ToGetStudentHomeworkQuery();
        var queryResponse = await _getStudentHomeworkHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetStudentHomeworkResponse();
    }

    public override async Task<V1SearchTeacherCoursesHomeworksResponse> V1SearchTeacherCoursesHomeworks(
        V1SearchTeacherCoursesHomeworksRequest request,
        ServerCallContext context)
    {
        var query = request.ToSearchTeacherCourseHomeworksQuery();
        var queryResponse = await _searchTeacherCourseHomeworksHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1SearchTeacherCoursesHomeworksResponse();
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
