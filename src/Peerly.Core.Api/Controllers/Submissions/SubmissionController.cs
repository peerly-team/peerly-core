using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateHomeworkSubmission;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Submissions;

[ExcludeFromCodeCoverage]
public sealed class SubmissionController : SubmissionService.SubmissionServiceBase
{
    private readonly ICommandHandler<CreateHomeworkSubmissionCommand, CreateHomeworkSubmissionCommandResponse> _createHomeworkSubmissionHandler;

    public SubmissionController(
        ICommandHandler<CreateHomeworkSubmissionCommand, CreateHomeworkSubmissionCommandResponse> createHomeworkSubmissionHandler)
    {
        _createHomeworkSubmissionHandler = createHomeworkSubmissionHandler;
    }

    public override async Task<V1CreateHomeworkSubmissionResponse> V1CreateHomeworkSubmission(
        V1CreateHomeworkSubmissionRequest request,
        ServerCallContext context)
    {
        var command = request.ToCreateHomeworkSubmissionCommand();
        var commandResponse = await _createHomeworkSubmissionHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1CreateHomeworkSubmissionResponse();
    }
}
