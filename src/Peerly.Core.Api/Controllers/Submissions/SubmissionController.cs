using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetStudentSubmission;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmissionForReview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetStudentCourseResults;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmissionDetail;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.SearchHomeworkSubmissions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.SearchStudentAssignedReviews;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Submissions;

[ExcludeFromCodeCoverage]
public sealed class SubmissionController : SubmissionService.SubmissionServiceBase
{
    private readonly ICommandHandler<CreateSubmittedHomeworkCommand, CreateSubmittedHomeworkCommandResponse> _createSubmittedHomeworkHandler;
    private readonly ICommandHandler<CreateSubmittedHomeworkFileCommand, CreateSubmittedHomeworkFileCommandResponse> _createSubmittedHomeworkFileHandler;
    private readonly ICommandHandler<CreateSubmittedReviewCommand, CreateSubmittedReviewCommandResponse> _createSubmittedReviewHandler;
    private readonly IQueryHandler<SearchHomeworkSubmissionsQuery, SearchHomeworkSubmissionsQueryResponse> _searchHomeworkSubmissionsHandler;
    private readonly IQueryHandler<GetStudentSubmissionQuery, GetStudentSubmissionQueryResponse> _getStudentSubmissionHandler;
    private readonly IQueryHandler<SearchStudentAssignedReviewsQuery, SearchStudentAssignedReviewsQueryResponse> _searchStudentAssignedReviewsHandler;
    private readonly IQueryHandler<GetSubmissionForReviewQuery, GetSubmissionForReviewQueryResponse> _getSubmissionForReviewHandler;
    private readonly IQueryHandler<GetTeacherSubmissionDetailQuery, GetTeacherSubmissionDetailQueryResponse> _getTeacherSubmissionDetailHandler;
    private readonly IQueryHandler<GetStudentCourseResultsQuery, GetStudentCourseResultsQueryResponse> _getStudentCourseResultsHandler;

    public SubmissionController(
        ICommandHandler<CreateSubmittedHomeworkCommand, CreateSubmittedHomeworkCommandResponse> createSubmittedHomeworkHandler,
        ICommandHandler<CreateSubmittedHomeworkFileCommand, CreateSubmittedHomeworkFileCommandResponse> createSubmittedHomeworkFileHandler,
        ICommandHandler<CreateSubmittedReviewCommand, CreateSubmittedReviewCommandResponse> createSubmittedReviewHandler,
        IQueryHandler<SearchHomeworkSubmissionsQuery, SearchHomeworkSubmissionsQueryResponse> searchHomeworkSubmissionsHandler,
        IQueryHandler<GetStudentSubmissionQuery, GetStudentSubmissionQueryResponse> getStudentSubmissionHandler,
        IQueryHandler<SearchStudentAssignedReviewsQuery, SearchStudentAssignedReviewsQueryResponse> searchStudentAssignedReviewsHandler,
        IQueryHandler<GetSubmissionForReviewQuery, GetSubmissionForReviewQueryResponse> getSubmissionForReviewHandler,
        IQueryHandler<GetTeacherSubmissionDetailQuery, GetTeacherSubmissionDetailQueryResponse> getTeacherSubmissionDetailHandler,
        IQueryHandler<GetStudentCourseResultsQuery, GetStudentCourseResultsQueryResponse> getStudentCourseResultsHandler)
    {
        _createSubmittedHomeworkHandler = createSubmittedHomeworkHandler;
        _createSubmittedHomeworkFileHandler = createSubmittedHomeworkFileHandler;
        _createSubmittedReviewHandler = createSubmittedReviewHandler;
        _searchHomeworkSubmissionsHandler = searchHomeworkSubmissionsHandler;
        _getStudentSubmissionHandler = getStudentSubmissionHandler;
        _searchStudentAssignedReviewsHandler = searchStudentAssignedReviewsHandler;
        _getSubmissionForReviewHandler = getSubmissionForReviewHandler;
        _getTeacherSubmissionDetailHandler = getTeacherSubmissionDetailHandler;
        _getStudentCourseResultsHandler = getStudentCourseResultsHandler;
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

    public override async Task<V1SearchHomeworkSubmissionsResponse> V1SearchHomeworkSubmissions(
        V1SearchHomeworkSubmissionsRequest request,
        ServerCallContext context)
    {
        var query = request.ToSearchHomeworkSubmissionsQuery();
        var queryResponse = await _searchHomeworkSubmissionsHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1SearchHomeworkSubmissionsResponse();
    }

    public override async Task<V1GetStudentSubmissionResponse> V1GetStudentSubmission(
        V1GetStudentSubmissionRequest request,
        ServerCallContext context)
    {
        var query = request.ToGetStudentSubmissionQuery();
        var queryResponse = await _getStudentSubmissionHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetStudentSubmissionResponse();
    }

    public override async Task<V1SearchStudentAssignedReviewsResponse> V1SearchStudentAssignedReviews(
        V1SearchStudentAssignedReviewsRequest request,
        ServerCallContext context)
    {
        var query = request.ToSearchStudentAssignedReviewsQuery();
        var queryResponse = await _searchStudentAssignedReviewsHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1SearchStudentAssignedReviewsResponse();
    }

    public override async Task<V1GetSubmissionForReviewResponse> V1GetSubmissionForReview(
        V1GetSubmissionForReviewRequest request,
        ServerCallContext context)
    {
        var query = request.ToGetSubmissionForReviewQuery();
        var queryResponse = await _getSubmissionForReviewHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetSubmissionForReviewResponse();
    }

    public override async Task<V1GetTeacherSubmissionDetailResponse> V1GetTeacherSubmissionDetail(
        V1GetTeacherSubmissionDetailRequest request,
        ServerCallContext context)
    {
        var query = request.ToGetTeacherSubmissionDetailQuery();
        var queryResponse = await _getTeacherSubmissionDetailHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetTeacherSubmissionDetailResponse();
    }

    public override async Task<V1GetStudentCourseResultsResponse> V1GetStudentCourseResults(
        V1GetStudentCourseResultsRequest request,
        ServerCallContext context)
    {
        var query = request.ToGetStudentCourseResultsQuery();
        var queryResponse = await _getStudentCourseResultsHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetStudentCourseResultsResponse();
    }
}
