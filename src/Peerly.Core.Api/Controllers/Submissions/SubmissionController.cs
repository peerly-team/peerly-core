using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedReview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetAssignedReview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmittedReview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.ListAssignedReviews;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.ListSubmittedHomeworkOverview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedReview;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Submissions;

[ExcludeFromCodeCoverage]
public sealed class SubmissionController : SubmissionService.SubmissionServiceBase
{
    private readonly ICommandHandler<CreateSubmittedHomeworkCommand, CreateSubmittedHomeworkCommandResponse> _createSubmittedHomeworkHandler;
    private readonly ICommandHandler<CreateSubmittedHomeworkFileCommand, CreateSubmittedHomeworkFileCommandResponse> _createSubmittedHomeworkFileHandler;
    private readonly ICommandHandler<CreateSubmittedReviewCommand, CreateSubmittedReviewCommandResponse> _createSubmittedReviewHandler;
    private readonly ICommandHandler<UpdateSubmittedHomeworkCommand, Success> _updateSubmittedHomeworkHandler;
    private readonly ICommandHandler<DeleteSubmittedHomeworkCommand, Success> _deleteSubmittedHomeworkHandler;
    private readonly ICommandHandler<DeleteSubmittedHomeworkFileCommand, Success> _deleteSubmittedHomeworkFileHandler;
    private readonly IQueryHandler<GetSubmittedHomeworkQuery, GetSubmittedHomeworkQueryResponse> _getSubmittedHomeworkHandler;
    private readonly IQueryHandler<ListAssignedReviewsQuery, ListAssignedReviewsQueryResponse> _listAssignedReviewsHandler;
    private readonly IQueryHandler<ListSubmittedHomeworkOverviewQuery, ListSubmittedHomeworkOverviewQueryResponse> _listSubmittedHomeworkOverviewHandler;
    private readonly IQueryHandler<GetAssignedReviewQuery, GetAssignedReviewQueryResponse> _getAssignedReviewHandler;
    private readonly ICommandHandler<UpdateSubmittedReviewCommand, Success> _updateSubmittedReviewHandler;
    private readonly ICommandHandler<DeleteSubmittedReviewCommand, Success> _deleteSubmittedReviewHandler;
    private readonly IQueryHandler<GetSubmittedReviewQuery, GetSubmittedReviewQueryResponse> _getSubmittedReviewHandler;
    private readonly IQueryHandler<GetTeacherSubmittedHomeworkQuery, GetTeacherSubmittedHomeworkQueryResponse> _getTeacherSubmittedHomeworkHandler;

    public SubmissionController(
        ICommandHandler<CreateSubmittedHomeworkCommand, CreateSubmittedHomeworkCommandResponse> createSubmittedHomeworkHandler,
        ICommandHandler<CreateSubmittedHomeworkFileCommand, CreateSubmittedHomeworkFileCommandResponse> createSubmittedHomeworkFileHandler,
        ICommandHandler<CreateSubmittedReviewCommand, CreateSubmittedReviewCommandResponse> createSubmittedReviewHandler,
        ICommandHandler<UpdateSubmittedHomeworkCommand, Success> updateSubmittedHomeworkHandler,
        ICommandHandler<DeleteSubmittedHomeworkCommand, Success> deleteSubmittedHomeworkHandler,
        ICommandHandler<DeleteSubmittedHomeworkFileCommand, Success> deleteSubmittedHomeworkFileHandler,
        IQueryHandler<GetSubmittedHomeworkQuery, GetSubmittedHomeworkQueryResponse> getSubmittedHomeworkHandler,
        IQueryHandler<ListAssignedReviewsQuery, ListAssignedReviewsQueryResponse> listAssignedReviewsHandler,
        IQueryHandler<ListSubmittedHomeworkOverviewQuery, ListSubmittedHomeworkOverviewQueryResponse> listSubmittedHomeworkOverviewHandler,
        IQueryHandler<GetAssignedReviewQuery, GetAssignedReviewQueryResponse> getAssignedReviewHandler,
        ICommandHandler<UpdateSubmittedReviewCommand, Success> updateSubmittedReviewHandler,
        ICommandHandler<DeleteSubmittedReviewCommand, Success> deleteSubmittedReviewHandler,
        IQueryHandler<GetSubmittedReviewQuery, GetSubmittedReviewQueryResponse> getSubmittedReviewHandler,
        IQueryHandler<GetTeacherSubmittedHomeworkQuery, GetTeacherSubmittedHomeworkQueryResponse> getTeacherSubmittedHomeworkHandler)
    {
        _createSubmittedHomeworkHandler = createSubmittedHomeworkHandler;
        _createSubmittedHomeworkFileHandler = createSubmittedHomeworkFileHandler;
        _createSubmittedReviewHandler = createSubmittedReviewHandler;
        _updateSubmittedHomeworkHandler = updateSubmittedHomeworkHandler;
        _deleteSubmittedHomeworkHandler = deleteSubmittedHomeworkHandler;
        _deleteSubmittedHomeworkFileHandler = deleteSubmittedHomeworkFileHandler;
        _getSubmittedHomeworkHandler = getSubmittedHomeworkHandler;
        _listAssignedReviewsHandler = listAssignedReviewsHandler;
        _listSubmittedHomeworkOverviewHandler = listSubmittedHomeworkOverviewHandler;
        _getAssignedReviewHandler = getAssignedReviewHandler;
        _updateSubmittedReviewHandler = updateSubmittedReviewHandler;
        _deleteSubmittedReviewHandler = deleteSubmittedReviewHandler;
        _getSubmittedReviewHandler = getSubmittedReviewHandler;
        _getTeacherSubmittedHomeworkHandler = getTeacherSubmittedHomeworkHandler;
    }

    public override async Task<V1CreateSubmittedHomeworkResponse> V1CreateSubmittedHomework(
        V1CreateSubmittedHomeworkRequest request,
        ServerCallContext context)
    {
        var command = request.ToCreateSubmittedHomeworkCommand();
        var commandResponse = await _createSubmittedHomeworkHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1CreateSubmittedHomeworkResponse();
    }

    public override async Task<V1CreateSubmittedHomeworkFileResponse> V1CreateSubmittedHomeworkFile(V1CreateSubmittedHomeworkFileRequest request, ServerCallContext context)
    {
        var command = request.ToCreateSubmittedHomeworkFileCommand();
        var commandResponse = await _createSubmittedHomeworkFileHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1CreateSubmittedHomeworkFileResponse();
    }

    public override async Task<V1CreateSubmittedReviewResponse> V1CreateSubmittedReview(
        V1CreateSubmittedReviewRequest request,
        ServerCallContext context)
    {
        var command = request.ToCreateSubmittedReviewCommand();
        var commandResponse = await _createSubmittedReviewHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1CreateSubmittedReviewResponse();
    }

    public override async Task<V1UpdateSubmittedHomeworkResponse> V1UpdateSubmittedHomework(
        V1UpdateSubmittedHomeworkRequest request,
        ServerCallContext context)
    {
        var command = request.ToUpdateSubmittedHomeworkCommand();
        var commandResponse = await _updateSubmittedHomeworkHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1UpdateSubmittedHomeworkResponse();
    }

    public override async Task<V1DeleteSubmittedHomeworkResponse> V1DeleteSubmittedHomework(
        V1DeleteSubmittedHomeworkRequest request,
        ServerCallContext context)
    {
        var command = request.ToDeleteSubmittedHomeworkCommand();
        var commandResponse = await _deleteSubmittedHomeworkHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1DeleteSubmittedHomeworkResponse();
    }

    public override async Task<V1DeleteSubmittedHomeworkFileResponse> V1DeleteSubmittedHomeworkFile(
        V1DeleteSubmittedHomeworkFileRequest request,
        ServerCallContext context)
    {
        var command = request.ToDeleteSubmittedHomeworkFileCommand();
        var commandResponse = await _deleteSubmittedHomeworkFileHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1DeleteSubmittedHomeworkFileResponse();
    }

    public override async Task<V1GetSubmittedHomeworkResponse> V1GetSubmittedHomework(
        V1GetSubmittedHomeworkRequest request,
        ServerCallContext context)
    {
        var query = request.ToGetSubmittedHomeworkQuery();
        var queryResponse = await _getSubmittedHomeworkHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetSubmittedHomeworkResponse();
    }

    public override async Task<V1ListAssignedReviewsResponse> V1ListAssignedReviews(
        V1ListAssignedReviewsRequest request,
        ServerCallContext context)
    {
        var query = request.ToListAssignedReviewsQuery();
        var queryResponse = await _listAssignedReviewsHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1ListAssignedReviewsResponse();
    }

    public override async Task<V1ListSubmittedHomeworkOverviewResponse> V1ListSubmittedHomeworkOverview(
        V1ListSubmittedHomeworkOverviewRequest request,
        ServerCallContext context)
    {
        var query = request.ToListSubmittedHomeworkOverviewQuery();
        var queryResponse = await _listSubmittedHomeworkOverviewHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1ListSubmittedHomeworkOverviewResponse();
    }

    public override async Task<V1GetAssignedReviewResponse> V1GetAssignedReview(
        V1GetAssignedReviewRequest request,
        ServerCallContext context)
    {
        var query = request.ToGetAssignedReviewQuery();
        var queryResponse = await _getAssignedReviewHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetAssignedReviewResponse();
    }

    public override async Task<V1UpdateSubmittedReviewResponse> V1UpdateSubmittedReview(
        V1UpdateSubmittedReviewRequest request,
        ServerCallContext context)
    {
        var command = request.ToUpdateSubmittedReviewCommand();
        var commandResponse = await _updateSubmittedReviewHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1UpdateSubmittedReviewResponse();
    }

    public override async Task<V1DeleteSubmittedReviewResponse> V1DeleteSubmittedReview(
        V1DeleteSubmittedReviewRequest request,
        ServerCallContext context)
    {
        var command = request.ToDeleteSubmittedReviewCommand();
        var commandResponse = await _deleteSubmittedReviewHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1DeleteSubmittedReviewResponse();
    }

    public override async Task<V1GetSubmittedReviewResponse> V1GetSubmittedReview(
        V1GetSubmittedReviewRequest request,
        ServerCallContext context)
    {
        var query = request.ToGetSubmittedReviewQuery();
        var queryResponse = await _getSubmittedReviewHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetSubmittedReviewResponse();
    }

    public override async Task<V1GetTeacherSubmittedHomeworkResponse> V1GetTeacherSubmittedHomework(
        V1GetTeacherSubmittedHomeworkRequest request,
        ServerCallContext context)
    {
        var query = request.ToGetTeacherSubmittedHomeworkQuery();
        var queryResponse = await _getTeacherSubmittedHomeworkHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetTeacherSubmittedHomeworkResponse();
    }
}
