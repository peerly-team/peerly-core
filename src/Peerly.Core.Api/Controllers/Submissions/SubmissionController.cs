using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Submissions;

[ExcludeFromCodeCoverage]
public sealed class SubmissionController : SubmissionService.SubmissionServiceBase
{
    private readonly ICommandHandler<CreateSubmittedHomeworkCommand, CreateSubmittedHomeworkCommandResponse> _createSubmittedHomeworkHandler;
    private readonly ICommandHandler<CreateSubmittedHomeworkFileCommand, CreateSubmittedHomeworkFileCommandResponse> _createSubmittedHomeworkFileHandler;

    public SubmissionController(
        ICommandHandler<CreateSubmittedHomeworkCommand, CreateSubmittedHomeworkCommandResponse> createSubmittedHomeworkHandler,
        ICommandHandler<CreateSubmittedHomeworkFileCommand, CreateSubmittedHomeworkFileCommandResponse> createSubmittedHomeworkFileHandler)
    {
        _createSubmittedHomeworkHandler = createSubmittedHomeworkHandler;
        _createSubmittedHomeworkFileHandler = createSubmittedHomeworkFileHandler;
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
}
